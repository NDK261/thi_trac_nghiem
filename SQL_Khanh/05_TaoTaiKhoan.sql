USE [THITRACNGHIEM];
GO

IF OBJECT_ID(N'dbo.SP_TAOLOGIN', N'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_TAOLOGIN;
GO

CREATE PROCEDURE dbo.SP_TAOLOGIN
    @LGNAME NVARCHAR(128),
    @PASS NVARCHAR(128),
    @USERNAME NVARCHAR(50),
    @ROLE NVARCHAR(30)
AS
/*
QUY ƯỚC MÃ LỖI (RETURN CODES):
  1: Tên đăng nhập trống
  2: Mật khẩu trống
  3: Tên User (Mã GV) trống hoặc vượt quá 8 ký tự
  4: Nhóm quyền (Role) truyền vào không hợp lệ
  5: Không tìm thấy Giáo viên đang hoạt động
  6: Tên Login đã tồn tại trên Server
  7: Tên User đã tồn tại dưới Database
  8: Nhóm quyền (Role) chưa được tạo trong hệ thống
 99: Lỗi Hệ thống: Tham số vượt quá chiều dài hoặc Giao dịch tạo tài khoản thất bại
  0: Thành công
*/
BEGIN
    SET NOCOUNT ON;

    DECLARE @LGNAME_CHUAN SYSNAME;
    DECLARE @PASS_CHUAN NVARCHAR(128);
    DECLARE @USERNAME_TXT NVARCHAR(50);
    DECLARE @USERNAME_CHUAN NCHAR(8);
    DECLARE @ROLE_CHUAN SYSNAME;
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @CreatedLogin BIT;
    DECLARE @CreatedUser BIT;

    SET @CreatedLogin = 0;
    SET @CreatedUser = 0;

    SET @LGNAME_CHUAN = LTRIM(RTRIM(ISNULL(@LGNAME, N'')));
    SET @PASS_CHUAN = LTRIM(RTRIM(ISNULL(@PASS, N'')));
    SET @USERNAME_TXT = UPPER(LTRIM(RTRIM(ISNULL(@USERNAME, N''))));
    SET @ROLE_CHUAN = UPPER(LTRIM(RTRIM(ISNULL(@ROLE, N''))));

    IF @LGNAME_CHUAN = N'' RETURN 1;
    IF @PASS_CHUAN = N'' RETURN 2;
    IF @USERNAME_TXT = N'' RETURN 3;
    IF @ROLE_CHUAN NOT IN (N'PGV', N'GIANGVIEN') RETURN 4;
    IF LEN(@USERNAME_TXT) > 8 RETURN 3;
    IF LEN(@LGNAME_CHUAN) > 128 OR LEN(@PASS_CHUAN) > 128 RETURN 99;

    SET @USERNAME_CHUAN = LEFT(@USERNAME_TXT + REPLICATE(N' ', 8), 8);

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.GIAOVIEN
        WHERE RTRIM(MAGV) = RTRIM(@USERNAME_CHUAN)
          AND TRANGTHAI = 1
    )
        RETURN 5;

    IF EXISTS (SELECT 1 FROM master.sys.server_principals WHERE name = @LGNAME_CHUAN)
        RETURN 6;

    IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = RTRIM(@USERNAME_CHUAN))
        RETURN 7;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.database_principals
        WHERE name = @ROLE_CHUAN
          AND type = N'R'
    )
        RETURN 8;

    IF IS_SRVROLEMEMBER(N'sysadmin') <> 1
       AND IS_SRVROLEMEMBER(N'securityadmin') <> 1
       AND HAS_PERMS_BY_NAME(NULL, NULL, N'ALTER ANY LOGIN') <> 1
        RETURN 9;

    BEGIN TRY
        SET @SQL = N'CREATE LOGIN ' + QUOTENAME(@LGNAME_CHUAN)
                 + N' WITH PASSWORD = ' + QUOTENAME(@PASS_CHUAN, '''')
                 + N', CHECK_POLICY = OFF, CHECK_EXPIRATION = OFF'
                 + N', DEFAULT_DATABASE = [THITRACNGHIEM];';
        EXEC (@SQL);
        SET @CreatedLogin = 1;

        SET @SQL = N'CREATE USER ' + QUOTENAME(RTRIM(@USERNAME_CHUAN))
                 + N' FOR LOGIN ' + QUOTENAME(@LGNAME_CHUAN) + N';';
        EXEC (@SQL);
        SET @CreatedUser = 1;

        SET @SQL = N'ALTER ROLE ' + QUOTENAME(@ROLE_CHUAN)
                 + N' ADD MEMBER ' + QUOTENAME(RTRIM(@USERNAME_CHUAN)) + N';';
        EXEC (@SQL);

        RETURN 0;
    END TRY
    BEGIN CATCH
        IF @CreatedUser = 1
        BEGIN
            SET @SQL = N'DROP USER ' + QUOTENAME(RTRIM(@USERNAME_CHUAN)) + N';';
            EXEC (@SQL);
        END

        IF @CreatedLogin = 1
        BEGIN
            SET @SQL = N'DROP LOGIN ' + QUOTENAME(@LGNAME_CHUAN) + N';';
            EXEC (@SQL);
        END

        RETURN 99;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_GET_GIAOVIEN_CHUA_CO_TAIKHOAN
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        MAGV = RTRIM(gv.MAGV),
        HOTEN = LTRIM(RTRIM(ISNULL(gv.HO, N''))) + N' ' + LTRIM(RTRIM(ISNULL(gv.TEN, N'')))
    FROM dbo.GIAOVIEN AS gv
    LEFT JOIN sys.database_principals AS dp
        ON dp.name = RTRIM(gv.MAGV)
    WHERE dp.name IS NULL
      AND gv.TRANGTHAI = 1
    ORDER BY gv.TEN, gv.HO, gv.MAGV;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_GET_GIAOVIEN_CO_TAIKHOAN
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        MAGV = RTRIM(gv.MAGV),
        HOTEN = LTRIM(RTRIM(ISNULL(gv.HO, N''))) + N' ' + LTRIM(RTRIM(ISNULL(gv.TEN, N'')))
    FROM dbo.GIAOVIEN AS gv
    INNER JOIN sys.database_principals AS dp
        ON dp.name = RTRIM(gv.MAGV)
    WHERE gv.TRANGTHAI = 1
    ORDER BY gv.TEN, gv.HO, gv.MAGV;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_XOALOGIN
    @USERNAME NVARCHAR(50)
AS
/*
RETURN CODES:
  1: Tên User truyền vào trống
  2: Không tìm thấy User dưới Database
  3: Không thể xóa chính mình đang đăng nhập
 99: Lỗi hệ thống
  0: Thành công
*/
BEGIN
    SET NOCOUNT ON;

    DECLARE @USERNAME_CHUAN NCHAR(8);
    DECLARE @LGNAME SYSNAME;
    DECLARE @SQL NVARCHAR(MAX);

    SET @USERNAME = LTRIM(RTRIM(ISNULL(@USERNAME, N'')));
    IF @USERNAME = N'' RETURN 1;

    SET @USERNAME_CHUAN = LEFT(@USERNAME + REPLICATE(N' ', 8), 8);

    IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = RTRIM(@USERNAME_CHUAN))
        RETURN 2;

    -- Lấy Login name tương ứng với Database user
    SELECT @LGNAME = suser_sname(sid)
    FROM sys.database_principals
    WHERE name = RTRIM(@USERNAME_CHUAN);

    -- Không cho phép xóa chính mình
    IF SUSER_SNAME() = @LGNAME
        RETURN 3;

    BEGIN TRY
        SET @SQL = N'DROP USER ' + QUOTENAME(RTRIM(@USERNAME_CHUAN)) + N';';
        EXEC (@SQL);

        SET @SQL = N'DROP LOGIN ' + QUOTENAME(@LGNAME) + N';';
        EXEC (@SQL);

        RETURN 0;
    END TRY
    BEGIN CATCH
        RETURN 99;
    END CATCH
END
GO
