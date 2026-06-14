USE [THITRACNGHIEM];
GO

IF COL_LENGTH(N'dbo.SINHVIEN', N'TRANGTHAI') IS NULL
BEGIN
    ALTER TABLE dbo.SINHVIEN
    ADD TRANGTHAI BIT NOT NULL
        CONSTRAINT DF_SINHVIEN_TRANGTHAI DEFAULT (1);
END
GO

IF OBJECT_ID(N'dbo.SP_DANGNHAP', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_DANGNHAP;
GO

CREATE PROCEDURE dbo.SP_DANGNHAP
    @TENLOGIN NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @LoginName SYSNAME;
    SET @LoginName = LTRIM(RTRIM(@TENLOGIN));

    SELECT TOP 1
        TENLOGIN = @LoginName,
        USERNAME = RTRIM(dp.name),
        HOTEN =
            CASE
                WHEN gv.MAGV IS NULL THEN RTRIM(dp.name)
                ELSE LTRIM(RTRIM(ISNULL(gv.HO, N''))) + N' ' + LTRIM(RTRIM(ISNULL(gv.TEN, N'')))
            END,
        TENNHOM = UPPER(rp.name)
    FROM sys.database_principals AS dp
    INNER JOIN sys.database_role_members AS drm
        ON drm.member_principal_id = dp.principal_id
    INNER JOIN sys.database_principals AS rp
        ON rp.principal_id = drm.role_principal_id
    LEFT JOIN dbo.GIAOVIEN AS gv
        ON RTRIM(gv.MAGV) = RTRIM(dp.name)
    WHERE dp.sid = SUSER_SID(@LoginName)
      AND rp.name IN (N'PGV', N'GIANGVIEN')
    ORDER BY CASE WHEN rp.name = N'PGV' THEN 1 ELSE 2 END;
END
GO

IF OBJECT_ID(N'dbo.SP_DANGNHAP_SINHVIEN', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_DANGNHAP_SINHVIEN;
GO

CREATE PROCEDURE dbo.SP_DANGNHAP_SINHVIEN
    @MASV NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MASV_CHUAN NCHAR(8);
    SET @MASV_CHUAN = UPPER(LEFT(LTRIM(RTRIM(ISNULL(@MASV, N''))) + REPLICATE(N' ', 8), 8));

    SELECT
        USERNAME = RTRIM(sv.MASV),
        HOTEN = LTRIM(RTRIM(ISNULL(sv.HO, N''))) + N' ' + LTRIM(RTRIM(ISNULL(sv.TEN, N''))),
        MALOP = RTRIM(sv.MALOP),
        TENNHOM = N'SINHVIEN'
    FROM dbo.SINHVIEN AS sv
    WHERE RTRIM(sv.MASV) = RTRIM(@MASV_CHUAN)
      AND sv.TRANGTHAI = 1;
END
GO
