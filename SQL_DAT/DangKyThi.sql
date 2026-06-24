USE [THITRACNGHIEM]
GO

-- Cap nhat lai rang buoc thoi gian thi cho dung khoang 5 den 60 phut.
IF OBJECT_ID(N'dbo.CK_THOIGIAN', N'C') IS NOT NULL
    ALTER TABLE dbo.GIAOVIEN_DANGKY DROP CONSTRAINT CK_THOIGIAN;
GO

ALTER TABLE dbo.GIAOVIEN_DANGKY
ADD CONSTRAINT CK_THOIGIAN CHECK (THOIGIAN >= 5 AND THOIGIAN <= 60);
GO

-- Lay danh sach lich thi de hien tren form Dang Ky Thi.
CREATE OR ALTER PROCEDURE [dbo].[SP_GET_DANGKYTHI]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT MAGV, MAMH, MALOP, TRINHDO, NGAYTHI, LAN, SOCAUTHI, THOIGIAN
    FROM GIAOVIEN_DANGKY
    ORDER BY NGAYTHI DESC, MAMH, MALOP, LAN;
END
GO

-- Kiem tra lich thi da co diem hoac dang co bai thi tam hay chua.
-- Form Dang Ky Thi dung SP nay de khoa sua/xoa truoc khi goi SP cap nhat.
CREATE OR ALTER PROCEDURE [dbo].[SP_KIEMTRA_DANGKY_DA_CO_SV_THI]
    @MAMH NCHAR(5),
    @MALOP NCHAR(15),
    @LAN SMALLINT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM BANGDIEM BD
        INNER JOIN SINHVIEN SV ON BD.MASV = SV.MASV
        WHERE BD.MAMH = @MAMH
          AND SV.MALOP = @MALOP
          AND BD.LAN = @LAN
    )
        SELECT 1 AS DaCoSinhVienThi;
    ELSE IF OBJECT_ID(N'dbo.BAITHI_TAM', N'U') IS NOT NULL
        AND EXISTS (
            SELECT 1
            FROM BAITHI_TAM BT
            WHERE BT.MAMH = @MAMH
              AND BT.MALOP = @MALOP
              AND BT.LAN = @LAN
        )
        SELECT 1 AS DaCoSinhVienThi;
    ELSE
        SELECT 0 AS DaCoSinhVienThi;
END
GO

-- Them lich thi moi: kiem tra ngay thi, trung lich va kho cau hoi.
CREATE OR ALTER PROCEDURE [dbo].[SP_THEM_DANGKYTHI]
    @MAGV NCHAR(8),
    @MAMH NCHAR(5),
    @MALOP NCHAR(15),
    @TRINHDO NCHAR(1),
    @NGAYTHI DATETIME,
    @LAN SMALLINT,
    @SOCAUTHI SMALLINT,
    @THOIGIAN SMALLINT
AS
BEGIN
    SET NOCOUNT ON;

    SET @TRINHDO = UPPER(LTRIM(RTRIM(@TRINHDO)));

    -- Ngay thi phai tu ngay mai tro di, lan thi chi nhan 1 hoac 2.
    IF @TRINHDO NOT IN (N'A', N'B', N'C')
       OR @LAN NOT IN (1, 2)
       OR @SOCAUTHI < 10 OR @SOCAUTHI > 100
       OR @THOIGIAN < 5 OR @THOIGIAN > 60
       OR @NGAYTHI IS NULL
       OR CAST(@NGAYTHI AS DATE) <= CAST(GETDATE() AS DATE)
        RETURN 4;

    IF EXISTS (
        SELECT 1
        FROM GIAOVIEN_DANGKY
        WHERE MAMH = @MAMH AND MALOP = @MALOP AND LAN = @LAN
    )
        RETURN 1;

    -- Kiểm tra ràng buộc ngày thi lần 1 và lần 2
    IF @LAN = 2
    BEGIN
        DECLARE @NgayThiLan1 DATETIME;
        SELECT @NgayThiLan1 = NGAYTHI 
        FROM GIAOVIEN_DANGKY 
        WHERE MAMH = @MAMH AND MALOP = @MALOP AND LAN = 1;

        IF @NgayThiLan1 IS NULL
            RETURN 5; -- Lần 1 chưa đăng ký
        IF CAST(@NGAYTHI AS DATE) < CAST(@NgayThiLan1 AS DATE)
            RETURN 5; -- Ngày thi lần 2 phải sau hoặc cùng ngày với lần 1
    END
    ELSE IF @LAN = 1
    BEGIN
        DECLARE @NgayThiLan2 DATETIME;
        SELECT @NgayThiLan2 = NGAYTHI 
        FROM GIAOVIEN_DANGKY 
        WHERE MAMH = @MAMH AND MALOP = @MALOP AND LAN = 2;

        IF @NgayThiLan2 IS NOT NULL AND CAST(@NGAYTHI AS DATE) > CAST(@NgayThiLan2 AS DATE)
            RETURN 5; -- Ngày thi lần 1 phải trước hoặc cùng ngày với lần 2
    END

    DECLARE @SoCauChinhCanCo INT = CEILING(@SOCAUTHI * 0.7);

    DECLARE @SoCauA INT = (SELECT COUNT(*) FROM BODE WHERE MAMH = @MAMH AND TRINHDO = N'A');
    DECLARE @SoCauB INT = (SELECT COUNT(*) FROM BODE WHERE MAMH = @MAMH AND TRINHDO = N'B');
    DECLARE @SoCauC INT = (SELECT COUNT(*) FROM BODE WHERE MAMH = @MAMH AND TRINHDO = N'C');

    -- Dang ky thi chi duoc tao khi kho cau hoi du theo luat 70/30.
    IF @TRINHDO = N'A' AND (@SoCauA < @SoCauChinhCanCo OR @SoCauA + @SoCauB < @SOCAUTHI)
        RETURN 2;

    IF @TRINHDO = N'B' AND (@SoCauB < @SoCauChinhCanCo OR @SoCauB + @SoCauC < @SOCAUTHI)
        RETURN 2;

    IF @TRINHDO = N'C' AND @SoCauC < @SOCAUTHI
        RETURN 2;

    INSERT INTO GIAOVIEN_DANGKY (MAGV, MAMH, MALOP, TRINHDO, NGAYTHI, LAN, SOCAUTHI, THOIGIAN)
    VALUES (@MAGV, @MAMH, @MALOP, @TRINHDO, @NGAYTHI, @LAN, @SOCAUTHI, @THOIGIAN);

    RETURN 0;
END
GO

-- Sua lich thi neu ca thi chua co diem hoac bai thi tam.
CREATE OR ALTER PROCEDURE [dbo].[SP_SUA_DANGKYTHI]
    @MAGV NCHAR(8),
    @MAMH NCHAR(5),
    @MALOP NCHAR(15),
    @TRINHDO NCHAR(1),
    @NGAYTHI DATETIME,
    @LAN SMALLINT,
    @SOCAUTHI SMALLINT,
    @THOIGIAN SMALLINT
AS
BEGIN
    SET NOCOUNT ON;

    SET @TRINHDO = UPPER(LTRIM(RTRIM(@TRINHDO)));

    -- NÂNG CẤP BẢO MẬT: Chỉ PGV hoặc chính giảng viên đã đăng ký ca thi này mới được sửa
    DECLARE @MAGV_CURRENT NCHAR(8);
    SET @MAGV_CURRENT = LEFT(USER_NAME() + REPLICATE(N' ', 8), 8);

    IF IS_MEMBER(N'PGV') = 0 
       AND IS_SRVROLEMEMBER(N'sysadmin') = 0
       AND NOT EXISTS (
           SELECT 1 
           FROM GIAOVIEN_DANGKY 
           WHERE MAMH = @MAMH AND MALOP = @MALOP AND LAN = @LAN AND RTRIM(MAGV) = RTRIM(@MAGV_CURRENT)
       )
    BEGIN
        RETURN 6; -- Không có quyền sửa
    END

    -- Kiểm tra ràng buộc ngày thi lần 1 và lần 2
    IF @LAN = 2
    BEGIN
        DECLARE @NgayThiLan1 DATETIME;
        SELECT @NgayThiLan1 = NGAYTHI 
        FROM GIAOVIEN_DANGKY 
        WHERE MAMH = @MAMH AND MALOP = @MALOP AND LAN = 1;

        IF @NgayThiLan1 IS NULL
            RETURN 5; -- Lần 1 chưa đăng ký
        IF CAST(@NGAYTHI AS DATE) < CAST(@NgayThiLan1 AS DATE)
            RETURN 5; -- Ngày thi lần 2 phải sau hoặc cùng ngày với lần 1
    END
    ELSE IF @LAN = 1
    BEGIN
        DECLARE @NgayThiLan2 DATETIME;
        SELECT @NgayThiLan2 = NGAYTHI 
        FROM GIAOVIEN_DANGKY 
        WHERE MAMH = @MAMH AND MALOP = @MALOP AND LAN = 2;

        IF @NgayThiLan2 IS NOT NULL AND CAST(@NGAYTHI AS DATE) > CAST(@NgayThiLan2 AS DATE)
            RETURN 5; -- Ngày thi lần 1 phải trước hoặc cùng ngày với lần 2
    END

    -- Da co diem hoac bai thi tam thi khong sua de tranh lech bai thi dang/da lam.
    IF EXISTS (
        SELECT 1
        FROM BANGDIEM BD
        INNER JOIN SINHVIEN SV ON BD.MASV = SV.MASV
        WHERE BD.MAMH = @MAMH
          AND BD.LAN = @LAN
          AND SV.MALOP = @MALOP
    )
        RETURN 3;

    IF OBJECT_ID(N'dbo.BAITHI_TAM', N'U') IS NOT NULL
       AND EXISTS (
           SELECT 1
           FROM BAITHI_TAM BT
           WHERE BT.MAMH = @MAMH
             AND BT.MALOP = @MALOP
             AND BT.LAN = @LAN
       )
        RETURN 3;

    IF @TRINHDO NOT IN (N'A', N'B', N'C')
       OR @LAN NOT IN (1, 2)
       OR @SOCAUTHI < 10 OR @SOCAUTHI > 100
       OR @THOIGIAN < 5 OR @THOIGIAN > 60
       OR @NGAYTHI IS NULL
       OR CAST(@NGAYTHI AS DATE) <= CAST(GETDATE() AS DATE)
        RETURN 4;

    DECLARE @SoCauChinhCanCo INT = CEILING(@SOCAUTHI * 0.7);
    DECLARE @SoCauA INT = (SELECT COUNT(*) FROM BODE WHERE MAMH = @MAMH AND TRINHDO = N'A');
    DECLARE @SoCauB INT = (SELECT COUNT(*) FROM BODE WHERE MAMH = @MAMH AND TRINHDO = N'B');
    DECLARE @SoCauC INT = (SELECT COUNT(*) FROM BODE WHERE MAMH = @MAMH AND TRINHDO = N'C');

    -- Sua lich cung phai kiem tra lai kho cau hoi, phong truong hop bo de da thay doi.
    IF @TRINHDO = N'A' AND (@SoCauA < @SoCauChinhCanCo OR @SoCauA + @SoCauB < @SOCAUTHI)
        RETURN 2;

    IF @TRINHDO = N'B' AND (@SoCauB < @SoCauChinhCanCo OR @SoCauB + @SoCauC < @SOCAUTHI)
        RETURN 2;

    IF @TRINHDO = N'C' AND @SoCauC < @SOCAUTHI
        RETURN 2;

    UPDATE GIAOVIEN_DANGKY
    SET MAGV = @MAGV,
        TRINHDO = @TRINHDO,
        NGAYTHI = @NGAYTHI,
        SOCAUTHI = @SOCAUTHI,
        THOIGIAN = @THOIGIAN
    WHERE MAMH = @MAMH AND MALOP = @MALOP AND LAN = @LAN;

    RETURN 0;
END
GO

-- Xoa lich thi neu chua co diem va chua co bai thi tam.
CREATE OR ALTER PROCEDURE [dbo].[SP_XOA_DANGKYTHI]
    @MAMH NCHAR(5),
    @MALOP NCHAR(15),
    @LAN SMALLINT
AS
BEGIN
    SET NOCOUNT ON;

    -- NÂNG CẤP BẢO MẬT: Chỉ PGV hoặc chính giảng viên đã đăng ký ca thi này mới được xóa
    DECLARE @MAGV_CURRENT NCHAR(8);
    SET @MAGV_CURRENT = LEFT(USER_NAME() + REPLICATE(N' ', 8), 8);

    IF IS_MEMBER(N'PGV') = 0 
       AND IS_SRVROLEMEMBER(N'sysadmin') = 0
       AND NOT EXISTS (
           SELECT 1 
           FROM GIAOVIEN_DANGKY 
           WHERE MAMH = @MAMH AND MALOP = @MALOP AND LAN = @LAN AND RTRIM(MAGV) = RTRIM(@MAGV_CURRENT)
       )
    BEGIN
        RETURN 2; -- Không có quyền xóa
    END

    -- Da co diem hoac bai thi tam thi khong xoa lich goc cua bai thi.
    IF EXISTS (
        SELECT 1
        FROM BANGDIEM BD
        INNER JOIN SINHVIEN SV ON BD.MASV = SV.MASV
        WHERE BD.MAMH = @MAMH
          AND BD.LAN = @LAN
          AND SV.MALOP = @MALOP
    )
        RETURN 1;

    IF OBJECT_ID(N'dbo.BAITHI_TAM', N'U') IS NOT NULL
       AND EXISTS (
           SELECT 1
           FROM BAITHI_TAM BT
           WHERE BT.MAMH = @MAMH
             AND BT.MALOP = @MALOP
             AND BT.LAN = @LAN
       )
        RETURN 1;

    -- Thêm kiểm tra: Nếu xóa Lần 1 mà Lần 2 vẫn đang tồn tại thì chặn lại
    IF @LAN = 1 AND EXISTS (
        SELECT 1 
        FROM GIAOVIEN_DANGKY 
        WHERE MAMH = @MAMH AND MALOP = @MALOP AND LAN = 2
    )
    BEGIN
        RETURN 3;
    END

    DELETE FROM GIAOVIEN_DANGKY
    WHERE MAMH = @MAMH AND MALOP = @MALOP AND LAN = @LAN;

    RETURN 0;
END
GO
