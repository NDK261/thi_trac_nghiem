USE [THITRACNGHIEM];
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'PGV' AND type = N'R')
    EXEC(N'CREATE ROLE [PGV]');
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'GIANGVIEN' AND type = N'R')
    EXEC(N'CREATE ROLE [GIANGVIEN]');
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'SINHVIEN' AND type = N'R')
    EXEC(N'CREATE ROLE [SINHVIEN]');
GO

USE master;
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'sv')
BEGIN
    EXEC(N'CREATE LOGIN [sv] WITH PASSWORD = ''123'', CHECK_POLICY = OFF, CHECK_EXPIRATION = OFF;');
END
GO

USE [THITRACNGHIEM];
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'sv')
BEGIN
    EXEC(N'CREATE USER [sv] FOR LOGIN [sv];');
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members AS drm
    INNER JOIN sys.database_principals AS r
        ON r.principal_id = drm.role_principal_id
    INNER JOIN sys.database_principals AS m
        ON m.principal_id = drm.member_principal_id
    WHERE r.name = N'SINHVIEN'
      AND m.name = N'sv'
)
BEGIN
    ALTER ROLE [SINHVIEN] ADD MEMBER [sv];
END
GO

GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE ON SCHEMA::dbo TO [PGV];
GRANT ALTER ANY USER TO [PGV];
GRANT ALTER ANY ROLE TO [PGV];
GO

-- PGV tao tai khoan giao vien qua SP_TAOLOGIN, trong SP co lenh CREATE LOGIN.
-- CREATE LOGIN la quyen cap SQL Server, khong phai chi quyen trong database.
-- Do do khi chay file nay bang sa/sysadmin, cap them ALTER ANY LOGIN cho cac login dang thuoc role PGV.
DECLARE @GrantAlterLoginSql NVARCHAR(MAX);
SET @GrantAlterLoginSql = N'';

SELECT @GrantAlterLoginSql = @GrantAlterLoginSql + N'
IF SUSER_ID(N''' + REPLACE(sp.name, '''', '''''') + N''') IS NOT NULL
    GRANT ALTER ANY LOGIN TO ' + QUOTENAME(sp.name) + N';'
FROM sys.database_role_members drm
INNER JOIN sys.database_principals r
    ON r.principal_id = drm.role_principal_id
INNER JOIN sys.database_principals m
    ON m.principal_id = drm.member_principal_id
INNER JOIN sys.server_principals sp
    ON sp.sid = m.sid
WHERE r.name = N'PGV'
  AND sp.type IN (N'S', N'U', N'G');

IF @GrantAlterLoginSql <> N''
BEGIN
    BEGIN TRY
        EXEC master.sys.sp_executesql @GrantAlterLoginSql;
    END TRY
    BEGIN CATCH
        PRINT N'Khong cap duoc ALTER ANY LOGIN cho PGV. Hay chay file phan quyen bang sa/sysadmin.';
    END CATCH
END
GO

IF OBJECT_ID(N'dbo.LOP', N'U') IS NOT NULL
BEGIN
    GRANT SELECT ON dbo.LOP TO [GIANGVIEN];
END
GO

IF OBJECT_ID(N'dbo.SINHVIEN', N'U') IS NOT NULL
BEGIN
    GRANT SELECT ON dbo.SINHVIEN TO [GIANGVIEN];
END
GO

IF OBJECT_ID(N'dbo.MONHOC', N'U') IS NOT NULL
BEGIN
    GRANT SELECT ON dbo.MONHOC TO [GIANGVIEN];
END
GO

IF OBJECT_ID(N'dbo.GIAOVIEN_DANGKY', N'U') IS NOT NULL
BEGIN
    GRANT SELECT ON dbo.GIAOVIEN_DANGKY TO [GIANGVIEN];
END
GO

IF OBJECT_ID(N'dbo.BANGDIEM', N'U') IS NOT NULL
BEGIN
    GRANT SELECT ON dbo.BANGDIEM TO [GIANGVIEN];
END
GO

IF OBJECT_ID(N'dbo.BODE', N'U') IS NOT NULL
BEGIN
    -- GIANGVIEN xem bo de qua SP_GET_BODE_THEO_MONHOC de SP loc theo MAGV.
    REVOKE SELECT, INSERT, UPDATE, DELETE ON dbo.BODE FROM [GIANGVIEN];
END
GO

-- Sinh vien dung chung login sv nen khong cap quyen doc/ghi bang truc tiep.
-- App van chay qua cac stored procedure duoc GRANT EXECUTE ben duoi.
IF OBJECT_ID(N'dbo.LOP', N'U') IS NOT NULL
    REVOKE SELECT, INSERT, UPDATE, DELETE ON dbo.LOP FROM [SINHVIEN];
GO

IF OBJECT_ID(N'dbo.MONHOC', N'U') IS NOT NULL
    REVOKE SELECT, INSERT, UPDATE, DELETE ON dbo.MONHOC FROM [SINHVIEN];
GO

IF OBJECT_ID(N'dbo.SINHVIEN', N'U') IS NOT NULL
    REVOKE SELECT, INSERT, UPDATE, DELETE ON dbo.SINHVIEN FROM [SINHVIEN];
GO

IF OBJECT_ID(N'dbo.GIAOVIEN_DANGKY', N'U') IS NOT NULL
    REVOKE SELECT, INSERT, UPDATE, DELETE ON dbo.GIAOVIEN_DANGKY FROM [SINHVIEN];
GO

IF OBJECT_ID(N'dbo.BANGDIEM', N'U') IS NOT NULL
    REVOKE SELECT, INSERT, UPDATE, DELETE ON dbo.BANGDIEM FROM [SINHVIEN];
GO

IF OBJECT_ID(N'dbo.BODE', N'U') IS NOT NULL
    REVOKE SELECT, INSERT, UPDATE, DELETE ON dbo.BODE FROM [SINHVIEN];
GO

IF OBJECT_ID(N'dbo.CT_BAITHI', N'U') IS NOT NULL
    REVOKE SELECT, INSERT, UPDATE, DELETE ON dbo.CT_BAITHI FROM [SINHVIEN];
GO

IF OBJECT_ID(N'dbo.BAITHI_TAM', N'U') IS NOT NULL
    REVOKE SELECT, INSERT, UPDATE, DELETE ON dbo.BAITHI_TAM FROM [SINHVIEN];
GO

IF OBJECT_ID(N'dbo.CT_BAITHI_TAM', N'U') IS NOT NULL
    REVOKE SELECT, INSERT, UPDATE, DELETE ON dbo.CT_BAITHI_TAM FROM [SINHVIEN];
GO

DECLARE @GrantList TABLE
(
    ProcName SYSNAME NOT NULL,
    RoleName SYSNAME NOT NULL
);

INSERT INTO @GrantList(ProcName, RoleName) VALUES
    (N'SP_DANGNHAP', N'PGV'),
    (N'SP_DANGNHAP', N'GIANGVIEN'),
    (N'SP_DANGNHAP_SINHVIEN', N'SINHVIEN'),

    (N'SP_GET_MONHOC', N'GIANGVIEN'),
    (N'SP_GET_LOP', N'GIANGVIEN'),
    (N'SP_GET_GIAOVIEN', N'GIANGVIEN'),
    (N'SP_GET_SINHVIEN_THEO_LOP', N'GIANGVIEN'),

    (N'SP_GET_BODE_THEO_MONHOC', N'GIANGVIEN'),
    (N'SP_THEM_BODE', N'GIANGVIEN'),
    (N'SP_SUA_BODE', N'GIANGVIEN'),
    (N'SP_XOA_BODE', N'GIANGVIEN'),

    (N'SP_GET_DANGKYTHI', N'GIANGVIEN'),
    (N'SP_KIEMTRA_DANGKY_DA_CO_SV_THI', N'GIANGVIEN'),
    (N'SP_THEM_DANGKYTHI', N'GIANGVIEN'),
    (N'SP_SUA_DANGKYTHI', N'GIANGVIEN'),
    (N'SP_XOA_DANGKYTHI', N'GIANGVIEN'),
    (N'SP_KIEMTRA_BODE_DANGKY', N'GIANGVIEN'),

    (N'SP_LAY_DE_THI', N'GIANGVIEN'),
    (N'SP_LAY_DE_THI', N'SINHVIEN'),
    (N'SP_GET_LOP_CUA_SINHVIEN', N'SINHVIEN'),
    (N'SP_KIEMTRA_SINHVIEN_DA_THI', N'SINHVIEN'),
    (N'SP_GET_THONGTIN_DANGKY_THI', N'SINHVIEN'),
    (N'SP_GET_MON_THI_CHUA_THI_THEO_NGAY', N'SINHVIEN'),
    (N'SP_GET_LAN_THI_CHUA_THI', N'SINHVIEN'),
    (N'SP_TAO_BAITHI_TAM', N'SINHVIEN'),
    (N'SP_LUU_CHI_TIET_BAITHI_TAM', N'SINHVIEN'),
    (N'SP_CAPNHAT_BAITHI_TAM', N'SINHVIEN'),
    (N'SP_GET_BAITHI_TAM_DANG_CO', N'SINHVIEN'),
    (N'SP_GET_CHI_TIET_BAITHI_TAM', N'SINHVIEN'),
    (N'SP_XOA_BAITHI_TAM', N'SINHVIEN'),

    (N'SP_GHI_DIEM_THI', N'SINHVIEN'),
    (N'SP_GHI_CHI_TIET_BAI_THI', N'SINHVIEN'),

    (N'SP_DS_KETQUA_SINHVIEN', N'GIANGVIEN'),
    (N'SP_DS_KETQUA_SINHVIEN', N'SINHVIEN'),
    (N'SP_XEM_KETQUA', N'GIANGVIEN'),
    (N'SP_XEM_KETQUA', N'SINHVIEN'),

    (N'SP_GET_BANGDIEM', N'GIANGVIEN'),
    (N'SP_IN_BANGDIEM', N'GIANGVIEN');

DECLARE @SQL NVARCHAR(MAX);
SET @SQL = N'';

SELECT @SQL = @SQL + N'
IF OBJECT_ID(N''dbo.' + REPLACE(ProcName, '''', '''''') + N''', N''P'') IS NOT NULL
    GRANT EXECUTE ON dbo.' + QUOTENAME(ProcName) + N' TO ' + QUOTENAME(RoleName) + N';'
FROM @GrantList;

EXEC sp_executesql @SQL;
GO

/*
Ghi chu quan trong:
- SP_TAOLOGIN co lenh CREATE LOGIN nen tai khoan dang goi procedure phai co quyen server-level de tao login.
- Trong luc demo, hay tao tai khoan PGV dau tien bang sa/Windows admin, sau do dang nhap PGV de tao GIANGVIEN.
Vi du:
    EXEC dbo.SP_TAOLOGIN N'pgv01', N'123456', N'TH101', N'PGV';
    EXEC dbo.SP_TAOLOGIN N'gv01',  N'123456', N'TH123', N'GIANGVIEN';
*/
