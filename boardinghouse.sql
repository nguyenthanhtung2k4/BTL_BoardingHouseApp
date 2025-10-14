-- 1️⃣ Tạo cơ sở dữ liệu
CREATE DATABASE BoardingHouse;
GO

-- Sử dụng cơ sở dữ liệu vừa tạo
USE BoardingHouse;
GO

-- 2️⃣ Bảng Phòng cho thuê (rooms)
CREATE TABLE Rooms (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    price DECIMAL(10,2) NOT NULL DEFAULT 0.0,
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE()
);
GO

-- 3️⃣ Bảng Người thuê (tenants)
CREATE TABLE Tenants (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE()
);
GO

-- 4️⃣ Bảng Hợp đồng thuê (contracts)
CREATE TABLE Contracts (
    id INT IDENTITY(1,1) PRIMARY KEY,
    tenant_id INT NOT NULL,
    room_id INT NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    status VARCHAR(20) NOT NULL CHECK (status IN ('hoạt động', 'hết hạn', 'chấm dứt')),
    created_at DATETIME DEFAULT GETDATE(),    updated_at DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Contracts_Tenants FOREIGN KEY (tenant_id) REFERENCES Tenants(id),
    CONSTRAINT FK_Contracts_Rooms FOREIGN KEY (room_id) REFERENCES Rooms(id),
    CONSTRAINT UQ_Contracts_Room UNIQUE (room_id)
);
GO

-- 5️⃣ Bảng Thanh toán (payments)
CREATE TABLE Payments (
    id INT IDENTITY(1,1) PRIMARY KEY,
    contract_id INT NOT NULL,
    amount DECIMAL(10,2) NOT NULL DEFAULT 0.0,
    payment_date DATE NOT NULL,
    payment_method VARCHAR(20) NOT NULL CHECK (payment_method IN ('tiền mặt', 'chuyển khoản', 'online')),
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Payments_Contracts FOREIGN KEY (contract_id) REFERENCES Contracts(id)
);
GO
