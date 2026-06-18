USE [THITRACNGHIEM];
GO

/*
    Script cho form Lop - Sinh vien.
    TRANGTHAI = 1 la dang hoc, TRANGTHAI = 0 la da xoa mem.
    RETURN: 0 thanh cong, 1 that bai, 2 truong hop dac biet, 3 du lieu nhap sai.
*/

-- Them cot TRANGTHAI neu database chua co.
IF COL_LENGTH(N'dbo.SINHVIEN', N'TRANGTHAI') IS NULL
BEGIN
    ALTER TABLE dbo.SINHVIEN
    ADD TRANGTHAI BIT NOT NULL
        CONSTRAINT DF_SINHVIEN_TRANGTHAI DEFAULT (1);
END
GO

-- Chan ALTER neu du lieu cu dang dai hon gioi han trong de tai.
IF EXISTS (SELECT 1 FROM dbo.SINHVIEN WHERE LEN(LTRIM(RTRIM(ISNULL(HO, N'')))) > 40)
BEGIN
    RAISERROR(N'Du lieu SINHVIEN.HO dang vuot 40 ky tu. Hay rut gon du lieu truoc khi chay script.', 16, 1);
END
GO

-- Dong bo cot HO theo schema de tai.
ALTER TABLE dbo.SINHVIEN ALTER COLUMN HO NVARCHAR(40) NULL;
GO

-- Lay sinh vien dang hoat dong theo lop.
CREATE OR ALTER PROCEDURE dbo.SP_GET_SINHVIEN_THEO_LOP
    @MALOP NCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;

    -- Trim de du lieu NCHAR hien thi gon hon tren form.
    SELECT
        MASV = RTRIM(MASV),
        HO = LTRIM(RTRIM(ISNULL(HO, N''))),
        TEN = LTRIM(RTRIM(ISNULL(TEN, N''))),
        NGAYSINH,
        DIACHI = LTRIM(RTRIM(ISNULL(DIACHI, N''))),
        MALOP = RTRIM(MALOP)
    FROM dbo.SINHVIEN
    WHERE RTRIM(MALOP) = RTRIM(@MALOP)
      AND TRANGTHAI = 1 -- Chi hien sinh vien dang hoc.
    ORDER BY TEN, HO, MASV;
END
GO

-- Lay sinh vien da xoa mem de co the khoi phuc.
CREATE OR ALTER PROCEDURE dbo.SP_GET_SINHVIEN_NGUNG_HOATDONG_THEO_LOP
    @MALOP NCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        MASV = RTRIM(MASV),
        HO = LTRIM(RTRIM(ISNULL(HO, N''))),
        TEN = LTRIM(RTRIM(ISNULL(TEN, N''))),
        NGAYSINH,
        DIACHI = LTRIM(RTRIM(ISNULL(DIACHI, N''))),
        MALOP = RTRIM(MALOP)
    FROM dbo.SINHVIEN
    WHERE RTRIM(MALOP) = RTRIM(@MALOP)
      AND TRANGTHAI = 0 -- Chi lay sinh vien da xoa mem.
    ORDER BY TEN, HO, MASV;
END
GO

-- Them sinh vien moi.
CREATE OR ALTER PROCEDURE dbo.SP_THEM_SINHVIEN
    @MASV NCHAR(8),
    @HO NVARCHAR(40),
    @TEN NVARCHAR(10),
    @NGAYSINH DATE,
    @DIACHI NVARCHAR(100),
    @MALOP NCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MASV_CHUAN NVARCHAR(8);
    DECLARE @MALOP_CHUAN NVARCHAR(15);

    -- Chuan hoa ma va chuoi nhap vao.
    SET @MASV_CHUAN = UPPER(LTRIM(RTRIM(ISNULL(@MASV, N''))));
    SET @MALOP_CHUAN = UPPER(LTRIM(RTRIM(ISNULL(@MALOP, N''))));
    SET @HO = LTRIM(RTRIM(@HO));
    SET @TEN = LTRIM(RTRIM(@TEN));
    SET @DIACHI = LTRIM(RTRIM(@DIACHI));

    -- Kiem tra du lieu truoc khi ghi xuong bang.
    IF @MASV_CHUAN = N'' OR @HO = N'' OR @TEN = N''
       OR LEN(@MASV_CHUAN) > 8 OR LEN(@HO) > 40 OR LEN(@TEN) > 10 OR LEN(@DIACHI) > 100
       OR @NGAYSINH > CAST(GETDATE() AS DATE)
       OR @MASV_CHUAN COLLATE Latin1_General_BIN2 LIKE N'%[^A-Z0-9]%'
       OR @HO LIKE N'%[0-9]%' OR @TEN LIKE N'%[0-9]%'
       OR @HO LIKE N'%[~!@#$%^&*()_+=`{}|:;"'',.<>/?\-]%'
       OR @TEN LIKE N'%[~!@#$%^&*()_+=`{}|:;"'',.<>/?\-]%'
       OR @HO LIKE N'%[[]%' OR @HO LIKE N'%]%'
       OR @TEN LIKE N'%[[]%' OR @TEN LIKE N'%]%'
        RETURN 3;

    -- Khong them trung MASV.
    IF EXISTS (SELECT 1 FROM dbo.SINHVIEN WHERE RTRIM(MASV) = @MASV_CHUAN)
        RETURN 1;

    -- MALOP phai co trong bang LOP.
    IF NOT EXISTS (SELECT 1 FROM dbo.LOP WHERE RTRIM(MALOP) = @MALOP_CHUAN)
        RETURN 2;

    -- Sinh vien moi mac dinh dang hoc.
    INSERT INTO dbo.SINHVIEN (MASV, HO, TEN, NGAYSINH, DIACHI, MALOP, TRANGTHAI)
    VALUES (@MASV_CHUAN, @HO, @TEN, @NGAYSINH, NULLIF(@DIACHI, N''), @MALOP_CHUAN, 1);

    RETURN 0;
END
GO

-- Sua thong tin sinh vien.
CREATE OR ALTER PROCEDURE dbo.SP_SUA_SINHVIEN
    @MASV NCHAR(8),
    @HO NVARCHAR(40),
    @TEN NVARCHAR(10),
    @NGAYSINH DATE,
    @DIACHI NVARCHAR(100),
    @MALOP NCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MASV_CHUAN NVARCHAR(8);
    DECLARE @MALOP_CHUAN NVARCHAR(15);

    -- Chuan hoa truoc khi so sanh va cap nhat.
    SET @MASV_CHUAN = UPPER(LTRIM(RTRIM(ISNULL(@MASV, N''))));
    SET @MALOP_CHUAN = UPPER(LTRIM(RTRIM(ISNULL(@MALOP, N''))));
    SET @HO = LTRIM(RTRIM(@HO));
    SET @TEN = LTRIM(RTRIM(@TEN));
    SET @DIACHI = LTRIM(RTRIM(@DIACHI));

    -- Tra 3 neu du lieu khong dat rang buoc.
    IF @MASV_CHUAN = N'' OR @HO = N'' OR @TEN = N''
       OR LEN(@MASV_CHUAN) > 8 OR LEN(@HO) > 40 OR LEN(@TEN) > 10 OR LEN(@DIACHI) > 100
       OR @NGAYSINH > CAST(GETDATE() AS DATE)
       OR @MASV_CHUAN COLLATE Latin1_General_BIN2 LIKE N'%[^A-Z0-9]%'
       OR @HO LIKE N'%[0-9]%' OR @TEN LIKE N'%[0-9]%'
       OR @HO LIKE N'%[~!@#$%^&*()_+=`{}|:;"'',.<>/?\-]%'
       OR @TEN LIKE N'%[~!@#$%^&*()_+=`{}|:;"'',.<>/?\-]%'
       OR @HO LIKE N'%[[]%' OR @HO LIKE N'%]%'
       OR @TEN LIKE N'%[[]%' OR @TEN LIKE N'%]%'
        RETURN 3;

    -- Chi sua sinh vien dang hoc.
    IF NOT EXISTS (SELECT 1 FROM dbo.SINHVIEN WHERE RTRIM(MASV) = @MASV_CHUAN AND TRANGTHAI = 1)
        RETURN 2;

    -- MALOP moi phai ton tai.
    IF NOT EXISTS (SELECT 1 FROM dbo.LOP WHERE RTRIM(MALOP) = @MALOP_CHUAN)
        RETURN 2;

    -- Cap nhat thong tin, giu nguyen MASV.
    UPDATE dbo.SINHVIEN
    SET HO = @HO,
        TEN = @TEN,
        NGAYSINH = @NGAYSINH,
        DIACHI = NULLIF(@DIACHI, N''),
        MALOP = @MALOP_CHUAN
    WHERE RTRIM(MASV) = @MASV_CHUAN
      AND TRANGTHAI = 1;

    RETURN 0;
END
GO

-- Xoa sinh vien.
CREATE OR ALTER PROCEDURE dbo.SP_XOA_SINHVIEN
    @MASV NCHAR(8)
AS
BEGIN
    SET NOCOUNT ON;

    -- Chuan hoa MASV.
    SET @MASV = UPPER(LTRIM(RTRIM(@MASV)));

    IF @MASV = N'' OR LEN(@MASV) > 8
        RETURN 1;

    -- Khong co sinh vien dang hoc thi dung.
    IF NOT EXISTS (SELECT 1 FROM dbo.SINHVIEN WHERE RTRIM(MASV) = RTRIM(@MASV) AND TRANGTHAI = 1)
        RETURN 1;

    -- Da co diem/bai thi thi xoa mem de giu lich su.
    IF EXISTS (SELECT 1 FROM dbo.BANGDIEM WHERE RTRIM(MASV) = RTRIM(@MASV))
       OR EXISTS (SELECT 1 FROM dbo.BAITHI_TAM WHERE RTRIM(MASV) = RTRIM(@MASV))
       OR EXISTS (SELECT 1 FROM dbo.CT_BAITHI WHERE RTRIM(MASV) = RTRIM(@MASV))
       OR EXISTS (SELECT 1 FROM dbo.CT_BAITHI_TAM WHERE RTRIM(MASV) = RTRIM(@MASV))
    BEGIN
        UPDATE dbo.SINHVIEN
        SET TRANGTHAI = 0
        WHERE RTRIM(MASV) = RTRIM(@MASV);

        RETURN 2;
    END;

    -- Chua co du lieu lien quan thi xoa han.
    DELETE FROM dbo.SINHVIEN
    WHERE RTRIM(MASV) = RTRIM(@MASV);

    RETURN 0;
END
GO

-- Khoi phuc sinh vien da xoa mem.
CREATE OR ALTER PROCEDURE dbo.SP_KHOIPHUC_SINHVIEN
    @MASV NCHAR(8)
AS
BEGIN
    SET NOCOUNT ON;

    -- Chuan hoa MASV.
    SET @MASV = UPPER(LTRIM(RTRIM(@MASV)));

    IF @MASV = N'' OR LEN(@MASV) > 8
        RETURN 1;

    -- Chi khoi phuc ban ghi dang xoa mem.
    IF NOT EXISTS (SELECT 1 FROM dbo.SINHVIEN WHERE RTRIM(MASV) = RTRIM(@MASV) AND TRANGTHAI = 0)
        RETURN 1;

    -- Doi ve trang thai dang hoc.
    UPDATE dbo.SINHVIEN
    SET TRANGTHAI = 1
    WHERE RTRIM(MASV) = RTRIM(@MASV);

    RETURN 0;
END
GO
