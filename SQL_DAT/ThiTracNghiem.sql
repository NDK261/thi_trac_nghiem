USE [THITRACNGHIEM]
GO

-- Lay lop cua sinh vien dang dang nhap de form Thi loc dung lich thi cua lop do.
CREATE OR ALTER PROCEDURE [dbo].[SP_GET_LOP_CUA_SINHVIEN]
    @MASV NCHAR(8)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT SV.MALOP, L.TENLOP
    FROM SINHVIEN SV
    INNER JOIN LOP L ON SV.MALOP = L.MALOP
    WHERE SV.MASV = @MASV;
END
GO

-- Kiem tra sinh vien da co diem cua mon va lan thi nay chua.
-- Neu da co diem thi khong cho thi lai cung mon/lan.
CREATE OR ALTER PROCEDURE [dbo].[SP_KIEMTRA_SINHVIEN_DA_THI]
    @MASV NCHAR(8),
    @MAMH NCHAR(5),
    @LAN SMALLINT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM BANGDIEM
        WHERE MASV = @MASV
          AND MAMH = @MAMH
          AND LAN = @LAN
    )
        SELECT 1 AS DaThi;
    ELSE
        SELECT 0 AS DaThi;
END
GO

-- Lay cau hinh ca thi dung mon, lop, lan va ngay sinh vien da chon.
CREATE OR ALTER PROCEDURE [dbo].[SP_GET_THONGTIN_DANGKY_THI]
    @MAMH NCHAR(5),
    @MALOP NCHAR(15),
    @LAN SMALLINT,
    @NGAYTHI DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT SOCAUTHI, THOIGIAN, TRINHDO, NGAYTHI
    FROM GIAOVIEN_DANGKY
    WHERE MAMH = @MAMH
      AND MALOP = @MALOP
      AND LAN = @LAN
      AND CAST(NGAYTHI AS DATE) = @NGAYTHI;
END
GO

-- Lay cac mon thi trong ngay da chon ma sinh vien chua thi.
CREATE OR ALTER PROCEDURE [dbo].[SP_GET_MON_THI_CHUA_THI_THEO_NGAY]
    @MASV NCHAR(8),
    @MALOP NCHAR(15),
    @NGAYTHI DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT MH.MAMH, MH.TENMH
    FROM GIAOVIEN_DANGKY DK
    INNER JOIN MONHOC MH ON DK.MAMH = MH.MAMH
    LEFT JOIN BANGDIEM BD
        ON BD.MASV = @MASV
       AND BD.MAMH = DK.MAMH
       AND BD.LAN = DK.LAN
    WHERE DK.MALOP = @MALOP
      AND CAST(DK.NGAYTHI AS DATE) = @NGAYTHI
      AND BD.MASV IS NULL
    ORDER BY MH.TENMH;
END
GO

-- Lay cac lan thi cua mot mon trong ngay da chon ma sinh vien chua thi.
CREATE OR ALTER PROCEDURE [dbo].[SP_GET_LAN_THI_CHUA_THI]
    @MASV NCHAR(8),
    @MALOP NCHAR(15),
    @MAMH NCHAR(5),
    @NGAYTHI DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DK.LAN, DK.NGAYTHI, DK.TRINHDO, DK.SOCAUTHI, DK.THOIGIAN
    FROM GIAOVIEN_DANGKY DK
    LEFT JOIN BANGDIEM BD
        ON BD.MASV = @MASV
       AND BD.MAMH = DK.MAMH
       AND BD.LAN = DK.LAN
    WHERE DK.MALOP = @MALOP
      AND DK.MAMH = @MAMH
      AND CAST(DK.NGAYTHI AS DATE) = @NGAYTHI
      AND BD.MASV IS NULL
    ORDER BY DK.LAN;
END
GO

-- Boc de thi ngau nhien theo mon, trinh do va so cau da dang ky.
CREATE OR ALTER PROCEDURE [dbo].[SP_LAY_DE_THI]
    @MAMH NCHAR(5),
    @TRINHDO NCHAR(1),
    @SOCAU SMALLINT
AS
BEGIN
    SET NOCOUNT ON;

    SET @TRINHDO = UPPER(LTRIM(RTRIM(@TRINHDO)));

    -- A duoc lay them B, B duoc lay them C, con C thi lay toan bo tu C.
    DECLARE @TrinhDoPhu NCHAR(1);

    IF @TRINHDO = N'A'
        SET @TrinhDoPhu = N'B';
    ELSE IF @TRINHDO = N'B'
        SET @TrinhDoPhu = N'C';
    ELSE
        SET @TrinhDoPhu = NULL;

    -- CEILING de dam bao phan trinh do chinh khong bi thieu 70%.
    DECLARE @SoCauChinh INT = CEILING(@SOCAU * 0.7);
    DECLARE @SoCauPhu INT = @SOCAU - @SoCauChinh;

    IF @TRINHDO = N'C'
    BEGIN
        SET @SoCauChinh = @SOCAU;
        SET @SoCauPhu = 0;
    END

    DECLARE @SoCauPhuCoSan INT = 0;

    IF @TrinhDoPhu IS NOT NULL
    BEGIN
        SELECT @SoCauPhuCoSan = COUNT(*)
        FROM BODE
        WHERE MAMH = @MAMH AND TRINHDO = @TrinhDoPhu;
    END

    IF @SoCauPhuCoSan < @SoCauPhu
    BEGIN
        SET @SoCauChinh = @SoCauChinh + (@SoCauPhu - @SoCauPhuCoSan);
        SET @SoCauPhu = @SoCauPhuCoSan;
    END

    -- ORDER BY NEWID() de boc cau ngau nhien, sau do tron lai thu tu hien thi.
    SELECT CAUHOI, NOIDUNG, A, B, C, D, DAP_AN
    FROM (
        SELECT CAUHOI, NOIDUNG, A, B, C, D, DAP_AN
        FROM (
            SELECT TOP (@SoCauChinh) CAUHOI, NOIDUNG, A, B, C, D, DAP_AN
            FROM BODE
            WHERE MAMH = @MAMH AND TRINHDO = @TRINHDO
            ORDER BY NEWID()
        ) AS CauChinh

        UNION ALL

        SELECT CAUHOI, NOIDUNG, A, B, C, D, DAP_AN
        FROM (
            SELECT TOP (@SoCauPhu) CAUHOI, NOIDUNG, A, B, C, D, DAP_AN
            FROM BODE
            WHERE MAMH = @MAMH AND TRINHDO = @TrinhDoPhu
            ORDER BY NEWID()
        ) AS CauPhu
    ) AS DeThi
    ORDER BY NEWID();
END
GO
