USE [THITRACNGHIEM];
GO

/*
================================================================================
BO SUNG PHAN THI SINH VIEN + RANG BUOC KHOA NGOAI

Muc tieu:
    1. Giu nguyen database hien tai, khong tao lai bang.
    2. Bo sung khoa ngoai con thieu de du lieu chat hon.
    3. Bo sung cac stored procedure phuc vu form Thi cua sinh vien.

Thu tu chay khuyen nghi:
    1. Chay file nay.
    2. Chay lai SQL_Khanh/09_PhanQuyen.sql de cap quyen EXECUTE cho SP moi.
================================================================================
*/

IF OBJECT_ID(N'dbo.FK_GIAOVIEN_DANGKY_GIAOVIEN', N'F') IS NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM dbo.GIAOVIEN_DANGKY AS dk
        WHERE dk.MAGV IS NOT NULL
          AND NOT EXISTS (
              SELECT 1
              FROM dbo.GIAOVIEN AS gv
              WHERE gv.MAGV = dk.MAGV
          )
    )
    BEGIN
        THROW 50001, N'Khong the tao FK_GIAOVIEN_DANGKY_GIAOVIEN vi GIAOVIEN_DANGKY co MAGV khong ton tai trong GIAOVIEN.', 1;
    END;

    ALTER TABLE dbo.GIAOVIEN_DANGKY WITH CHECK
    ADD CONSTRAINT FK_GIAOVIEN_DANGKY_GIAOVIEN
    FOREIGN KEY (MAGV) REFERENCES dbo.GIAOVIEN(MAGV);

    ALTER TABLE dbo.GIAOVIEN_DANGKY CHECK CONSTRAINT FK_GIAOVIEN_DANGKY_GIAOVIEN;
END;
GO

IF OBJECT_ID(N'dbo.FK_BAITHI_TAM_DANGKY', N'F') IS NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM dbo.BAITHI_TAM AS bt
        WHERE NOT EXISTS (
            SELECT 1
            FROM dbo.GIAOVIEN_DANGKY AS dk
            WHERE dk.MALOP = bt.MALOP
              AND dk.MAMH = bt.MAMH
              AND dk.LAN = bt.LAN
        )
    )
    BEGIN
        THROW 50002, N'Khong the tao FK_BAITHI_TAM_DANGKY vi BAITHI_TAM co ky thi khong ton tai trong GIAOVIEN_DANGKY.', 1;
    END;

    ALTER TABLE dbo.BAITHI_TAM WITH CHECK
    ADD CONSTRAINT FK_BAITHI_TAM_DANGKY
    FOREIGN KEY (MAMH, MALOP, LAN)
    REFERENCES dbo.GIAOVIEN_DANGKY(MAMH, MALOP, LAN);

    ALTER TABLE dbo.BAITHI_TAM CHECK CONSTRAINT FK_BAITHI_TAM_DANGKY;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_GET_LOP_CUA_SINHVIEN
    @MASV NCHAR(8)
AS
BEGIN
    SET NOCOUNT ON;

    SET @MASV = LTRIM(RTRIM(@MASV));

    SELECT
        MALOP = RTRIM(sv.MALOP),
        TENLOP = LTRIM(RTRIM(l.TENLOP))
    FROM dbo.SINHVIEN AS sv
    INNER JOIN dbo.LOP AS l
        ON l.MALOP = sv.MALOP
    WHERE sv.MASV = @MASV
      AND sv.TRANGTHAI = 1;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_KIEMTRA_SINHVIEN_DA_THI
    @MASV NCHAR(8),
    @MAMH NCHAR(5),
    @LAN SMALLINT
AS
BEGIN
    SET NOCOUNT ON;

    SET @MASV = LTRIM(RTRIM(@MASV));
    SET @MAMH = UPPER(LTRIM(RTRIM(@MAMH)));

    IF @LAN NOT IN (1, 2)
    BEGIN
        SELECT CAST(1 AS BIT) AS DaThi;
        RETURN;
    END;

    IF EXISTS (
        SELECT 1
        FROM dbo.BANGDIEM
        WHERE MASV = @MASV
          AND MAMH = @MAMH
          AND LAN = @LAN
    )
        SELECT CAST(1 AS BIT) AS DaThi;
    ELSE
        SELECT CAST(0 AS BIT) AS DaThi;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_GET_THONGTIN_DANGKY_THI
    @MAMH NCHAR(5),
    @MALOP NCHAR(15),
    @LAN SMALLINT,
    @NGAYTHI DATE
AS
BEGIN
    SET NOCOUNT ON;

    SET @MAMH = UPPER(LTRIM(RTRIM(@MAMH)));
    SET @MALOP = UPPER(LTRIM(RTRIM(@MALOP)));

    IF @LAN NOT IN (1, 2)
    BEGIN
        SELECT
            SOCAUTHI = CAST(NULL AS SMALLINT),
            THOIGIAN = CAST(NULL AS SMALLINT),
            TRINHDO = CAST(NULL AS CHAR(1)),
            NGAYTHI = CAST(NULL AS DATETIME)
        WHERE 1 = 0;
        RETURN;
    END;

    SELECT
        SOCAUTHI,
        THOIGIAN,
        TRINHDO,
        NGAYTHI
    FROM dbo.GIAOVIEN_DANGKY
    WHERE MAMH = @MAMH
      AND MALOP = @MALOP
      AND LAN = @LAN
      AND CAST(NGAYTHI AS DATE) = @NGAYTHI;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_GET_MON_THI_CHUA_THI_THEO_NGAY
    @MASV NCHAR(8),
    @MALOP NCHAR(15),
    @NGAYTHI DATE
AS
BEGIN
    SET NOCOUNT ON;

    SET @MASV = LTRIM(RTRIM(@MASV));
    SET @MALOP = UPPER(LTRIM(RTRIM(@MALOP)));

    SELECT DISTINCT
        MAMH = RTRIM(mh.MAMH),
        TENMH = LTRIM(RTRIM(mh.TENMH))
    FROM dbo.GIAOVIEN_DANGKY AS dk
    INNER JOIN dbo.MONHOC AS mh
        ON mh.MAMH = dk.MAMH
    LEFT JOIN dbo.BANGDIEM AS bd
        ON bd.MASV = @MASV
       AND bd.MAMH = dk.MAMH
       AND bd.LAN = dk.LAN
    WHERE dk.MALOP = @MALOP
      AND CAST(dk.NGAYTHI AS DATE) = @NGAYTHI
      AND bd.MASV IS NULL
    ORDER BY TENMH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_GET_LAN_THI_CHUA_THI
    @MASV NCHAR(8),
    @MALOP NCHAR(15),
    @MAMH NCHAR(5),
    @NGAYTHI DATE
AS
BEGIN
    SET NOCOUNT ON;

    SET @MASV = LTRIM(RTRIM(@MASV));
    SET @MALOP = UPPER(LTRIM(RTRIM(@MALOP)));
    SET @MAMH = UPPER(LTRIM(RTRIM(@MAMH)));

    SELECT
        dk.LAN,
        dk.NGAYTHI,
        dk.TRINHDO,
        dk.SOCAUTHI,
        dk.THOIGIAN
    FROM dbo.GIAOVIEN_DANGKY AS dk
    LEFT JOIN dbo.BANGDIEM AS bd
        ON bd.MASV = @MASV
       AND bd.MAMH = dk.MAMH
       AND bd.LAN = dk.LAN
    WHERE dk.MALOP = @MALOP
      AND dk.MAMH = @MAMH
      AND dk.LAN IN (1, 2)
      AND CAST(dk.NGAYTHI AS DATE) = @NGAYTHI
      AND bd.MASV IS NULL
    ORDER BY dk.LAN;
END;
GO
