USE [THITRACNGHIEM]
GO

-- Lay danh sach mon hoc de do len form va ComboBox.
CREATE OR ALTER PROCEDURE [dbo].[SP_GET_MONHOC]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT MAMH, TENMH
    FROM MONHOC
    ORDER BY MAMH;
END
GO

-- Them mon hoc moi, co kiem tra trung ma va trung ten.
CREATE OR ALTER PROCEDURE [dbo].[SP_THEM_MONHOC]
    @MAMH NCHAR(5),
    @TENMH NVARCHAR(40)
AS
BEGIN
    SET NOCOUNT ON;

    -- Chuan hoa du lieu truoc khi kiem tra trung.
    SET @MAMH = UPPER(LTRIM(RTRIM(@MAMH)));
    SET @TENMH = LTRIM(RTRIM(@TENMH));

    IF @MAMH = N'' OR @TENMH = N'' OR LEN(@MAMH) > 5 OR LEN(@TENMH) > 40
       OR RTRIM(@MAMH) COLLATE Latin1_General_BIN2 LIKE N'%[^A-Z0-9]%'
        RETURN 3;

    IF EXISTS (SELECT 1 FROM MONHOC WHERE MAMH = @MAMH)
        RETURN 1;

    IF EXISTS (
        SELECT 1
        FROM MONHOC
        WHERE UPPER(LTRIM(RTRIM(TENMH))) = UPPER(@TENMH)
    )
        RETURN 2;

    INSERT INTO MONHOC (MAMH, TENMH)
    VALUES (@MAMH, @TENMH);

    RETURN 0;
END
GO

-- Sua ten mon hoc theo ma mon dang chon.
CREATE OR ALTER PROCEDURE [dbo].[SP_SUA_MONHOC]
    @MAMH NCHAR(5),
    @TENMH NVARCHAR(40)
AS
BEGIN
    SET NOCOUNT ON;

    SET @MAMH = UPPER(LTRIM(RTRIM(@MAMH)));
    SET @TENMH = LTRIM(RTRIM(@TENMH));

    IF @MAMH = N'' OR @TENMH = N'' OR LEN(@MAMH) > 5 OR LEN(@TENMH) > 40
       OR RTRIM(@MAMH) COLLATE Latin1_General_BIN2 LIKE N'%[^A-Z0-9]%'
        RETURN 3;

    IF NOT EXISTS (SELECT 1 FROM MONHOC WHERE MAMH = @MAMH)
        RETURN 4;

    IF EXISTS (
        SELECT 1
        FROM MONHOC
        WHERE MAMH <> @MAMH
          AND UPPER(LTRIM(RTRIM(TENMH))) = UPPER(@TENMH)
    )
        RETURN 2;

    UPDATE MONHOC
    SET TENMH = @TENMH
    WHERE MAMH = @MAMH;

    RETURN 0;
END
GO

-- Xoa mon hoc neu chua phat sinh cau hoi, lich thi hoac diem.
CREATE OR ALTER PROCEDURE [dbo].[SP_XOA_MONHOC]
    @MAMH NCHAR(5)
AS
BEGIN
    SET NOCOUNT ON;

    SET @MAMH = UPPER(LTRIM(RTRIM(@MAMH)));

    -- Neu mon da duoc dung o bang khac thi khong xoa de giu du lieu lien quan.
    IF EXISTS (SELECT 1 FROM BODE WHERE MAMH = @MAMH)
        RETURN 1;

    IF EXISTS (SELECT 1 FROM GIAOVIEN_DANGKY WHERE MAMH = @MAMH)
        RETURN 2;

    IF EXISTS (SELECT 1 FROM BANGDIEM WHERE MAMH = @MAMH)
        RETURN 3;

    DELETE FROM MONHOC
    WHERE MAMH = @MAMH;

    IF @@ROWCOUNT = 0
        RETURN 4;

    RETURN 0;
END
GO
