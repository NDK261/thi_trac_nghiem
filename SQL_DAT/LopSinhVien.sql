USE [THITRACNGHIEM];
GO

/*
    Script phuc vu form Sinh vien.

    Y tuong xu ly:
    - Sinh vien binh thuong co TRANGTHAI = 1.
    - Sinh vien bi ngung hoat dong co TRANGTHAI = 0.
    - Neu sinh vien chua co du lieu thi/diem thi thi co the xoa han khoi bang SINHVIEN.
    - Neu sinh vien da co diem hoac dang co bai thi tam thi khong xoa han,
      chi doi TRANGTHAI = 0 de giu lai lich su bai thi.

    Cac ma RETURN hay dung:
    - 0: thao tac thanh cong.
    - 1: du lieu khong hop le, khong tim thay sinh vien, hoac khong the xoa han.
    - 2: voi them/sua la loi lop hoac sinh vien khong ton tai;
         voi xoa la da chuyen sang xoa mem.
    - 3: du lieu nhap khong hop le khi them/sua sinh vien.
*/

-- Database cu co the chua co cot TRANGTHAI, nen bo sung neu thieu.
IF COL_LENGTH(N'dbo.SINHVIEN', N'TRANGTHAI') IS NULL
BEGIN
    ALTER TABLE dbo.SINHVIEN
    ADD TRANGTHAI BIT NOT NULL
        CONSTRAINT DF_SINHVIEN_TRANGTHAI DEFAULT (1);
END
GO

-- Neu du lieu cu dang vuot 40 ky tu thi dung script lai de tranh cat mat ho ten.
IF EXISTS (SELECT 1 FROM dbo.SINHVIEN WHERE LEN(LTRIM(RTRIM(ISNULL(HO, N'')))) > 40)
BEGIN
    RAISERROR(N'Du lieu SINHVIEN.HO dang vuot 40 ky tu. Hay rut gon du lieu truoc khi chay script.', 16, 1);
END
GO

-- Dong bo lai kich thuoc cot HO theo schema trong de tai.
ALTER TABLE dbo.SINHVIEN ALTER COLUMN HO NVARCHAR(40) NULL;
GO

-- Tao lai SP lay danh sach sinh vien dang hoat dong theo lop.
IF OBJECT_ID(N'dbo.SP_GET_SINHVIEN_THEO_LOP', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_GET_SINHVIEN_THEO_LOP;
GO

CREATE PROCEDURE dbo.SP_GET_SINHVIEN_THEO_LOP
    @MALOP NCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;

    -- RTRIM/LTRIM giup du lieu NCHAR hien thi sach hon tren DataGridView.
    SELECT
        MASV = RTRIM(MASV),
        HO = LTRIM(RTRIM(ISNULL(HO, N''))),
        TEN = LTRIM(RTRIM(ISNULL(TEN, N''))),
        NGAYSINH,
        DIACHI = LTRIM(RTRIM(ISNULL(DIACHI, N''))),
        MALOP = RTRIM(MALOP)
    FROM dbo.SINHVIEN
    WHERE RTRIM(MALOP) = RTRIM(@MALOP)
      AND TRANGTHAI = 1 -- Chi hien sinh vien dang hoat dong.
    ORDER BY TEN, HO, MASV;
END
GO

-- SP nay dung cho che do xem/khoi phuc sinh vien da ngung hoat dong.
IF OBJECT_ID(N'dbo.SP_GET_SINHVIEN_NGUNG_HOATDONG_THEO_LOP', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_GET_SINHVIEN_NGUNG_HOATDONG_THEO_LOP;
GO

CREATE PROCEDURE dbo.SP_GET_SINHVIEN_NGUNG_HOATDONG_THEO_LOP
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
      AND TRANGTHAI = 0 -- Chi lay cac sinh vien da bi ngung hoat dong.
    ORDER BY TEN, HO, MASV;
END
GO

-- Tao lai SP them sinh vien.
IF OBJECT_ID(N'dbo.SP_THEM_SINHVIEN', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_THEM_SINHVIEN;
GO

CREATE PROCEDURE dbo.SP_THEM_SINHVIEN
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

    -- Chuan hoa ma de tranh loi do nhap du khoang trang hoac sai chu hoa/thuong.
    SET @MASV_CHUAN = UPPER(LTRIM(RTRIM(ISNULL(@MASV, N''))));
    SET @MALOP_CHUAN = UPPER(LTRIM(RTRIM(ISNULL(@MALOP, N''))));
    SET @HO = LTRIM(RTRIM(@HO));
    SET @TEN = LTRIM(RTRIM(@TEN));
    SET @DIACHI = LTRIM(RTRIM(@DIACHI));

    -- Kiem tra du lieu dau vao truoc khi INSERT vao SINHVIEN.
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

    -- Khong duoc them trung ma sinh vien.
    IF EXISTS (SELECT 1 FROM dbo.SINHVIEN WHERE RTRIM(MASV) = @MASV_CHUAN)
        RETURN 1;

    -- Lop phai ton tai truoc khi gan sinh vien vao lop do.
    IF NOT EXISTS (SELECT 1 FROM dbo.LOP WHERE RTRIM(MALOP) = @MALOP_CHUAN)
        RETURN 2;

    -- Dia chi rong thi luu NULL, con sinh vien moi mac dinh dang hoat dong.
    INSERT INTO dbo.SINHVIEN (MASV, HO, TEN, NGAYSINH, DIACHI, MALOP, TRANGTHAI)
    VALUES (@MASV_CHUAN, @HO, @TEN, @NGAYSINH, NULLIF(@DIACHI, N''), @MALOP_CHUAN, 1);

    RETURN 0;
END
GO

-- Tao lai SP sua thong tin sinh vien.
IF OBJECT_ID(N'dbo.SP_SUA_SINHVIEN', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_SUA_SINHVIEN;
GO

CREATE PROCEDURE dbo.SP_SUA_SINHVIEN
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

    -- Chuan hoa du lieu truoc khi so sanh va cap nhat.
    SET @MASV_CHUAN = UPPER(LTRIM(RTRIM(ISNULL(@MASV, N''))));
    SET @MALOP_CHUAN = UPPER(LTRIM(RTRIM(ISNULL(@MALOP, N''))));
    SET @HO = LTRIM(RTRIM(@HO));
    SET @TEN = LTRIM(RTRIM(@TEN));
    SET @DIACHI = LTRIM(RTRIM(@DIACHI));

    -- Neu du lieu nhap khong hop le thi tra 3 de form hien thong bao.
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

    -- Chi cho sua sinh vien dang hoat dong.
    IF NOT EXISTS (SELECT 1 FROM dbo.SINHVIEN WHERE RTRIM(MASV) = @MASV_CHUAN AND TRANGTHAI = 1)
        RETURN 2;

    -- Lop moi phai ton tai trong bang LOP.
    IF NOT EXISTS (SELECT 1 FROM dbo.LOP WHERE RTRIM(MALOP) = @MALOP_CHUAN)
        RETURN 2;

    -- Cap nhat thong tin theo MASV, khong doi ma sinh vien.
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

-- Tao lai SP xoa sinh vien.
IF OBJECT_ID(N'dbo.SP_XOA_SINHVIEN', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_XOA_SINHVIEN;
GO

CREATE PROCEDURE dbo.SP_XOA_SINHVIEN
    @MASV NCHAR(8)
AS
BEGIN
    SET NOCOUNT ON;

    -- Chuan hoa ma sinh vien truoc khi kiem tra.
    SET @MASV = UPPER(LTRIM(RTRIM(@MASV)));

    IF @MASV = N'' OR LEN(@MASV) > 8
        RETURN 1;

    -- Khong tim thay sinh vien dang hoat dong thi khong xoa.
    IF NOT EXISTS (SELECT 1 FROM dbo.SINHVIEN WHERE RTRIM(MASV) = RTRIM(@MASV) AND TRANGTHAI = 1)
        RETURN 1;

    -- Neu sinh vien da co du lieu thi/diem thi thi chi xoa mem de giu lich su.
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

    -- Sinh vien chua co du lieu lien quan thi co the xoa han.
    DELETE FROM dbo.SINHVIEN
    WHERE RTRIM(MASV) = RTRIM(@MASV);

    RETURN 0;
END
GO

-- Tao lai SP khoi phuc sinh vien da bi xoa mem.
IF OBJECT_ID(N'dbo.SP_KHOIPHUC_SINHVIEN', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_KHOIPHUC_SINHVIEN;
GO

CREATE PROCEDURE dbo.SP_KHOIPHUC_SINHVIEN
    @MASV NCHAR(8)
AS
BEGIN
    SET NOCOUNT ON;

    -- Chuan hoa ma sinh vien can khoi phuc.
    SET @MASV = UPPER(LTRIM(RTRIM(@MASV)));

    IF @MASV = N'' OR LEN(@MASV) > 8
        RETURN 1;

    -- Chi khoi phuc sinh vien dang o trang thai ngung hoat dong.
    IF NOT EXISTS (SELECT 1 FROM dbo.SINHVIEN WHERE RTRIM(MASV) = RTRIM(@MASV) AND TRANGTHAI = 0)
        RETURN 1;

    -- Doi lai trang thai de sinh vien hien trong danh sach dang hoat dong.
    UPDATE dbo.SINHVIEN
    SET TRANGTHAI = 1
    WHERE RTRIM(MASV) = RTRIM(@MASV);

    RETURN 0;
END
GO
