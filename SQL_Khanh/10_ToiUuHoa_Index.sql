USE [THITRACNGHIEM]
GO

/*
================================================================================
TỐI ƯU HÓA HIỆU NĂNG - TẠO COVERING INDEX
Giải quyết vấn đề: 
Bảng CT_BAITHI và CT_BAITHI_TAM phình to sẽ làm quá tải RAM khi đọc dữ liệu.
Tạo một Non-Clustered Index siêu nhẹ (chỉ chứa các cột cần thiết) để bao phủ hoàn 
toàn câu lệnh truy vấn trong SP_XEM_KETQUA và SP_GET_CHI_TIET_BAITHI_TAM.
Giúp Query Optimizer bốc thẳng dữ liệu từ RAM mà không cần chọc vào bảng vật lý.
================================================================================
*/

-- 1. Xóa Index cũ nếu đã tồn tại để tránh lỗi khi chạy nhiều lần
IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IDX_Covering_XemKetQua')
    DROP INDEX IDX_Covering_XemKetQua ON dbo.CT_BAITHI;
GO

IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IDX_Covering_XemKetQua_Tam')
    DROP INDEX IDX_Covering_XemKetQua_Tam ON dbo.CT_BAITHI_TAM;
GO

-- 2. Tạo Covering Index cho bảng thật (CT_BAITHI)
-- Chìa khóa tìm kiếm (Key columns): MASV, MAMH, LAN
-- Hành lý mang kèm (Include columns): CAUHOI, STT, DAP_AN_SV
CREATE NONCLUSTERED INDEX IDX_Covering_XemKetQua 
ON dbo.CT_BAITHI (MASV, MAMH, LAN)
INCLUDE (CAUHOI, STT, DAP_AN_SV);
GO

-- 3. Tạo Covering Index cho bảng nháp (CT_BAITHI_TAM)
CREATE NONCLUSTERED INDEX IDX_Covering_XemKetQua_Tam 
ON dbo.CT_BAITHI_TAM (MASV, MAMH, LAN)
INCLUDE (CAUHOI, STT, DAP_AN_SV);
GO

PRINT N'Đã triển khai thành công 2 Covering Index tối ưu hóa tốc độ O(log N)!';
GO
