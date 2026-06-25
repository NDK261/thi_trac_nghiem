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

-- Lay danh sach ca thi hop le cua lop sinh vien
CREATE OR ALTER PROCEDURE [dbo].[SP_GET_LICH_THI_HOP_LE_CUA_SINHVIEN]
    @MASV NCHAR(8),
    @MALOP NCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DK.MAMH, 
           MH.TENMH, 
           DK.LAN, 
           DK.TRINHDO, 
           DK.SOCAUTHI, 
           DK.THOIGIAN, 
           DK.NGAYTHI
    FROM GIAOVIEN_DANGKY DK
    INNER JOIN MONHOC MH ON DK.MAMH = MH.MAMH
    LEFT JOIN BANGDIEM BD_CURRENT 
        ON BD_CURRENT.MASV = @MASV 
       AND BD_CURRENT.MAMH = DK.MAMH 
       AND BD_CURRENT.LAN = DK.LAN
    WHERE DK.MALOP = @MALOP
      AND BD_CURRENT.MASV IS NULL -- Sinh vien chua thi lan nay
      -- Quy tac Lần 1 / Lần 2:
      -- Neu la Lần 2 thi chi hien khi Lần 1 chua tung duoc dang ky hoac Lần 1 da thi xong (co diem)
      AND (
          DK.LAN = 1 
          OR (
              DK.LAN = 2 
              AND EXISTS (
                  SELECT 1 FROM BANGDIEM BD1 
                  WHERE BD1.MASV = @MASV 
                    AND BD1.MAMH = DK.MAMH 
                    AND BD1.LAN = 1
              )
          )
      )
    ORDER BY DK.NGAYTHI ASC, MH.TENMH ASC, DK.LAN ASC;
END
GO

