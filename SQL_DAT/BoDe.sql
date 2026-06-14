USE [THITRACNGHIEM]
GO

-- Lay cau hoi theo mon hoc dang chon trong form Bo De.
CREATE OR ALTER PROCEDURE [dbo].[SP_GET_BODE_THEO_MONHOC]
    @MAMH NCHAR(5),
    @MAGV NCHAR(8) = NULL,
    @NHOM NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MAGV_CHUAN NCHAR(8);
    DECLARE @NHOM_CHUAN NVARCHAR(20);

    SET @MAGV_CHUAN = UPPER(LEFT(LTRIM(RTRIM(ISNULL(@MAGV, N''))) + REPLICATE(N' ', 8), 8));
    SET @NHOM_CHUAN = UPPER(LTRIM(RTRIM(ISNULL(@NHOM, N''))));

    SELECT CAUHOI, MAMH, TRINHDO, NOIDUNG, A, B, C, D, DAP_AN, MAGV
    FROM BODE
    WHERE MAMH = @MAMH
      AND (
            @NHOM_CHUAN = N'PGV'
            OR IS_MEMBER(N'PGV') = 1
            OR IS_SRVROLEMEMBER(N'sysadmin') = 1
            OR RTRIM(MAGV) = RTRIM(@MAGV_CHUAN)
          )
    ORDER BY CAUHOI;
END
GO

-- Them cau hoi vao BODE, SQL van kiem tra lai du lieu de tranh nhap sai.
CREATE OR ALTER PROCEDURE [dbo].[SP_THEM_BODE]
    @MAMH NCHAR(5),
    @TRINHDO NCHAR(1),
    -- Cot BODE.NOIDUNG chi luu 200 ky tu; de 500 o tham so de SP tu bat loi qua dai.
    @NOIDUNG NVARCHAR(500),
    @A NVARCHAR(200),
    @B NVARCHAR(200),
    @C NVARCHAR(200),
    @D NVARCHAR(200),
    @DAP_AN NCHAR(1),
    @MAGV NCHAR(8)
AS
BEGIN
    SET NOCOUNT ON;

    SET @TRINHDO = UPPER(LTRIM(RTRIM(@TRINHDO)));
    SET @DAP_AN = UPPER(LTRIM(RTRIM(@DAP_AN)));
    SET @NOIDUNG = LTRIM(RTRIM(@NOIDUNG));
    SET @A = LTRIM(RTRIM(@A));
    SET @B = LTRIM(RTRIM(@B));
    SET @C = LTRIM(RTRIM(@C));
    SET @D = LTRIM(RTRIM(@D));

    IF @TRINHDO NOT IN (N'A', N'B', N'C') OR @DAP_AN NOT IN (N'A', N'B', N'C', N'D')
        RETURN 4;

    IF @NOIDUNG = N'' OR @A = N'' OR @B = N'' OR @C = N'' OR @D = N''
        RETURN 4;

    IF LEN(@NOIDUNG) > 200
       OR LEN(@A) > 200 OR LEN(@B) > 200 OR LEN(@C) > 200 OR LEN(@D) > 200
        RETURN 4;

    IF @A = @B OR @A = @C OR @A = @D OR @B = @C OR @B = @D OR @C = @D
        RETURN 4;

    -- CAUHOI la cot IDENTITY nen khong truyen gia tri vao khi them moi.
    INSERT INTO BODE (MAMH, TRINHDO, NOIDUNG, A, B, C, D, DAP_AN, MAGV)
    VALUES (@MAMH, @TRINHDO, @NOIDUNG, @A, @B, @C, @D, @DAP_AN, @MAGV);

    RETURN 0;
END
GO

-- Chi giao vien tao cau hoi va cau chua duoc thi moi duoc sua.
CREATE OR ALTER PROCEDURE [dbo].[SP_SUA_BODE]
    @CAUHOI INT,
    @MAMH NCHAR(5),
    @TRINHDO NCHAR(1),
    -- Cot BODE.NOIDUNG chi luu 200 ky tu; de 500 o tham so de SP tu bat loi qua dai.
    @NOIDUNG NVARCHAR(500),
    @A NVARCHAR(200),
    @B NVARCHAR(200),
    @C NVARCHAR(200),
    @D NVARCHAR(200),
    @DAP_AN NCHAR(1),
    @MAGV NCHAR(8)
AS
BEGIN
    SET NOCOUNT ON;

    SET @TRINHDO = UPPER(LTRIM(RTRIM(@TRINHDO)));
    SET @DAP_AN = UPPER(LTRIM(RTRIM(@DAP_AN)));
    SET @NOIDUNG = LTRIM(RTRIM(@NOIDUNG));
    SET @A = LTRIM(RTRIM(@A));
    SET @B = LTRIM(RTRIM(@B));
    SET @C = LTRIM(RTRIM(@C));
    SET @D = LTRIM(RTRIM(@D));

    IF NOT EXISTS (SELECT 1 FROM BODE WHERE CAUHOI = @CAUHOI AND MAGV = @MAGV)
        RETURN 2;

    -- Cau da nam trong bai thi that thi giu nguyen de khong lam sai lich su bai lam.
    IF OBJECT_ID(N'dbo.CT_BAITHI', N'U') IS NOT NULL
    BEGIN
        DECLARE @SoLanDaDung INT = 0;

        EXEC sp_executesql
            N'SELECT @KQ = COUNT(*) FROM dbo.CT_BAITHI WHERE CAUHOI = @CAUHOI',
            N'@CAUHOI INT, @KQ INT OUTPUT',
            @CAUHOI = @CAUHOI,
            @KQ = @SoLanDaDung OUTPUT;

        IF @SoLanDaDung > 0
            RETURN 3;
    END

    -- Cau dang nam trong bai thi tam cung khong sua de de thi dang lam khong bi doi.
    IF OBJECT_ID(N'dbo.CT_BAITHI_TAM', N'U') IS NOT NULL
    BEGIN
        DECLARE @SoLanDangThi INT = 0;

        EXEC sp_executesql
            N'SELECT @KQ = COUNT(*) FROM dbo.CT_BAITHI_TAM WHERE CAUHOI = @CAUHOI',
            N'@CAUHOI INT, @KQ INT OUTPUT',
            @CAUHOI = @CAUHOI,
            @KQ = @SoLanDangThi OUTPUT;

        IF @SoLanDangThi > 0
            RETURN 3;
    END

    IF @TRINHDO NOT IN (N'A', N'B', N'C') OR @DAP_AN NOT IN (N'A', N'B', N'C', N'D')
        RETURN 4;

    IF @NOIDUNG = N'' OR @A = N'' OR @B = N'' OR @C = N'' OR @D = N''
        RETURN 4;

    IF LEN(@NOIDUNG) > 200
       OR LEN(@A) > 200 OR LEN(@B) > 200 OR LEN(@C) > 200 OR LEN(@D) > 200
        RETURN 4;

    IF @A = @B OR @A = @C OR @A = @D OR @B = @C OR @B = @D OR @C = @D
        RETURN 4;

    UPDATE BODE
    SET MAMH = @MAMH,
        TRINHDO = @TRINHDO,
        NOIDUNG = @NOIDUNG,
        A = @A,
        B = @B,
        C = @C,
        D = @D,
        DAP_AN = @DAP_AN
    WHERE CAUHOI = @CAUHOI;

    RETURN 0;
END
GO

-- Chi xoa cau hoi khi dung giao vien tao va cau chua nam trong bai thi.
CREATE OR ALTER PROCEDURE [dbo].[SP_XOA_BODE]
    @CAUHOI INT,
    @MAGV NCHAR(8)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM BODE WHERE CAUHOI = @CAUHOI AND MAGV = @MAGV)
        RETURN 2;

    -- Cau da nam trong bai thi that thi giu nguyen de khong lam sai lich su bai lam.
    IF OBJECT_ID(N'dbo.CT_BAITHI', N'U') IS NOT NULL
    BEGIN
        DECLARE @SoLanDaDung INT = 0;

        EXEC sp_executesql
            N'SELECT @KQ = COUNT(*) FROM dbo.CT_BAITHI WHERE CAUHOI = @CAUHOI',
            N'@CAUHOI INT, @KQ INT OUTPUT',
            @CAUHOI = @CAUHOI,
            @KQ = @SoLanDaDung OUTPUT;

        IF @SoLanDaDung > 0
            RETURN 1;
    END

    -- Cau dang nam trong bai thi tam cung khong xoa de sinh vien co the thi tiep.
    IF OBJECT_ID(N'dbo.CT_BAITHI_TAM', N'U') IS NOT NULL
    BEGIN
        DECLARE @SoLanDangThi INT = 0;

        EXEC sp_executesql
            N'SELECT @KQ = COUNT(*) FROM dbo.CT_BAITHI_TAM WHERE CAUHOI = @CAUHOI',
            N'@CAUHOI INT, @KQ INT OUTPUT',
            @CAUHOI = @CAUHOI,
            @KQ = @SoLanDangThi OUTPUT;

        IF @SoLanDangThi > 0
            RETURN 1;
    END

    DELETE FROM BODE
    WHERE CAUHOI = @CAUHOI;

    RETURN 0;
END
GO
