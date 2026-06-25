USE [THITRACNGHIEM];
GO

IF OBJECT_ID(N'dbo.SP_DS_KETQUA_SINHVIEN', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_DS_KETQUA_SINHVIEN;
GO

CREATE PROCEDURE dbo.SP_DS_KETQUA_SINHVIEN
    @MASV NCHAR(8)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MASV_CHUAN NCHAR(8);
    SET @MASV_CHUAN = UPPER(LEFT(LTRIM(RTRIM(ISNULL(@MASV, N''))) + REPLICATE(N' ', 8), 8));

    IF EXISTS (SELECT 1 FROM dbo.SINHVIEN WHERE RTRIM(MASV) = RTRIM(@MASV_CHUAN))
    BEGIN
        SELECT DISTINCT
            MASV = RTRIM(sv.MASV),
            HOTEN = LTRIM(RTRIM(ISNULL(sv.HO, N''))) + N' ' + LTRIM(RTRIM(ISNULL(sv.TEN, N''))),
            MALOP = RTRIM(sv.MALOP),
            TENLOP = LTRIM(RTRIM(l.TENLOP)),
            MAMH = RTRIM(bd.MAMH),
            TENMH = LTRIM(RTRIM(mh.TENMH)),
            TRINHDO = dk.TRINHDO,
            LAN = bd.LAN,
            NGAYTHI = bd.NGAYTHI
        FROM dbo.BANGDIEM AS bd
        INNER JOIN dbo.SINHVIEN AS sv
            ON sv.MASV = bd.MASV
        INNER JOIN dbo.LOP AS l
            ON l.MALOP = sv.MALOP
        INNER JOIN dbo.MONHOC AS mh
            ON mh.MAMH = bd.MAMH
        LEFT JOIN dbo.GIAOVIEN_DANGKY AS dk
            ON dk.MAMH = bd.MAMH
           AND dk.MALOP = sv.MALOP
           AND dk.LAN = bd.LAN
        WHERE RTRIM(bd.MASV) = RTRIM(@MASV_CHUAN)
          AND EXISTS (
              SELECT 1
              FROM dbo.CT_BAITHI AS ct
              WHERE ct.MASV = bd.MASV
                AND ct.MAMH = bd.MAMH
                AND ct.LAN = bd.LAN
          )
        ORDER BY NGAYTHI DESC, TENMH, LAN;

        RETURN;
    END

    IF IS_MEMBER(N'PGV') = 1 OR IS_MEMBER(N'GIANGVIEN') = 1 OR IS_SRVROLEMEMBER(N'sysadmin') = 1
    BEGIN
        SELECT DISTINCT
            MASV = RTRIM(sv.MASV),
            HOTEN = LTRIM(RTRIM(ISNULL(sv.HO, N''))) + N' ' + LTRIM(RTRIM(ISNULL(sv.TEN, N''))),
            MALOP = RTRIM(sv.MALOP),
            TENLOP = LTRIM(RTRIM(l.TENLOP)),
            MAMH = RTRIM(bd.MAMH),
            TENMH = LTRIM(RTRIM(mh.TENMH)) + N' - ' + RTRIM(sv.MASV) + N' - '
                    + LTRIM(RTRIM(ISNULL(sv.HO, N''))) + N' ' + LTRIM(RTRIM(ISNULL(sv.TEN, N''))),
            TRINHDO = dk.TRINHDO,
            LAN = bd.LAN,
            NGAYTHI = bd.NGAYTHI
        FROM dbo.BANGDIEM AS bd
        INNER JOIN dbo.SINHVIEN AS sv
            ON sv.MASV = bd.MASV
        INNER JOIN dbo.LOP AS l
            ON l.MALOP = sv.MALOP
        INNER JOIN dbo.MONHOC AS mh
            ON mh.MAMH = bd.MAMH
        LEFT JOIN dbo.GIAOVIEN_DANGKY AS dk
            ON dk.MAMH = bd.MAMH
           AND dk.MALOP = sv.MALOP
           AND dk.LAN = bd.LAN
        WHERE EXISTS (
              SELECT 1
              FROM dbo.CT_BAITHI AS ct
              WHERE ct.MASV = bd.MASV
                AND ct.MAMH = bd.MAMH
                AND ct.LAN = bd.LAN
          )
        ORDER BY NGAYTHI DESC, TENMH, LAN, MASV;

        RETURN;
    END

    SELECT TOP 0
        MASV = CAST(N'' AS NVARCHAR(8)),
        HOTEN = CAST(N'' AS NVARCHAR(60)),
        MALOP = CAST(N'' AS NVARCHAR(15)),
        TENLOP = CAST(N'' AS NVARCHAR(40)),
        MAMH = CAST(N'' AS NVARCHAR(5)),
        TENMH = CAST(N'' AS NVARCHAR(120)),
        TRINHDO = CAST(N'' AS NCHAR(1)),
        LAN = CAST(0 AS SMALLINT),
        NGAYTHI = CAST(NULL AS DATE);
END
GO

IF OBJECT_ID(N'dbo.SP_XEM_KETQUA', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_XEM_KETQUA;
GO

CREATE PROCEDURE dbo.SP_XEM_KETQUA
    @MASV NCHAR(8),
    @MAMH NCHAR(5),
    @LAN SMALLINT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MASV_CHUAN NCHAR(8), @MAMH_CHUAN NCHAR(5);
    SET @MASV_CHUAN = UPPER(LEFT(LTRIM(RTRIM(ISNULL(@MASV, N''))) + REPLICATE(N' ', 8), 8));
    SET @MAMH_CHUAN = UPPER(LEFT(LTRIM(RTRIM(ISNULL(@MAMH, N''))) + REPLICATE(N' ', 5), 5));

    IF @LAN NOT IN (1, 2)
    BEGIN
        SELECT TOP 0
            LOP = CAST(N'' AS NVARCHAR(40)),
            HOTEN = CAST(N'' AS NVARCHAR(60)),
            MASV = CAST(N'' AS NVARCHAR(8)),
            MONTHI = CAST(N'' AS NVARCHAR(40)),
            NGAYTHI = CAST(NULL AS DATE),
            LAN = CAST(0 AS SMALLINT),
            TRINHDO = CAST(N'' AS NCHAR(1)),
            STT_HIENTHI = CAST(0 AS INT),
            CAUSO = CAST(0 AS INT),
            NOIDUNG = CAST(N'' AS NVARCHAR(MAX)),
            A = CAST(N'' AS NVARCHAR(MAX)),
            B = CAST(N'' AS NVARCHAR(MAX)),
            C = CAST(N'' AS NVARCHAR(MAX)),
            D = CAST(N'' AS NVARCHAR(MAX)),
            TRALOI_SV = CAST(N'' AS NCHAR(1)),
            DAP_AN = CAST(N'' AS NCHAR(1));
        RETURN;
    END

    SELECT
        LOP = LTRIM(RTRIM(l.TENLOP)),
        HOTEN = LTRIM(RTRIM(ISNULL(sv.HO, N''))) + N' ' + LTRIM(RTRIM(ISNULL(sv.TEN, N''))),
        MASV = RTRIM(sv.MASV),
        MONTHI = LTRIM(RTRIM(mh.TENMH)),
        NGAYTHI = bd.NGAYTHI,
        LAN = bd.LAN,
        TRINHDO = dk.TRINHDO,
        STT_HIENTHI = ct.STT,
        CAUSO = ct.CAUHOI,
        NOIDUNG = b.NOIDUNG,
        A = b.A,
        B = b.B,
        C = b.C,
        D = b.D,
        TRALOI_SV = ct.DAP_AN_SV,
        DAP_AN = b.DAP_AN
    FROM dbo.CT_BAITHI AS ct
    INNER JOIN dbo.BANGDIEM AS bd
        ON bd.MASV = ct.MASV
       AND bd.MAMH = ct.MAMH
       AND bd.LAN = ct.LAN
    INNER JOIN dbo.BODE AS b
        ON b.CAUHOI = ct.CAUHOI
    INNER JOIN dbo.SINHVIEN AS sv
        ON sv.MASV = bd.MASV
    INNER JOIN dbo.LOP AS l
        ON l.MALOP = sv.MALOP
    INNER JOIN dbo.MONHOC AS mh
        ON mh.MAMH = bd.MAMH
    LEFT JOIN dbo.GIAOVIEN_DANGKY AS dk
        ON dk.MAMH = bd.MAMH
       AND dk.MALOP = sv.MALOP
       AND dk.LAN = bd.LAN
    WHERE RTRIM(ct.MASV) = RTRIM(@MASV_CHUAN)
      AND RTRIM(ct.MAMH) = RTRIM(@MAMH_CHUAN)
      AND ct.LAN = @LAN
    ORDER BY ct.STT, ct.CAUHOI;
END
GO

GRANT EXECUTE ON [dbo].[SP_DS_KETQUA_SINHVIEN] TO sv;
GRANT EXECUTE ON [dbo].[SP_XEM_KETQUA] TO sv;
GO
