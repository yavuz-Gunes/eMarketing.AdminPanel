CREATE OR ALTER PROCEDURE dbo.sp_Siparis_Ekle_TekUrun
    @CustomerName NVARCHAR(300),
    @CustomerEmail NVARCHAR(400) = NULL,
    @CustomerPhone NVARCHAR(100) = NULL,
    @ProductId INT,
    @Quantity INT,
    @TotalPrice DECIMAL(18,2),
    @CustomerStoreId INT = NULL,
    @OrderType NVARCHAR(50) = N'Bayi',
    @OrderSource NVARCHAR(50) = N'AdminPanel',
    @BayiYetkiliId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CustomerId INT;
    DECLARE @OrderId INT;
    DECLARE @CurrentStock INT;
    DECLARE @UnitPrice DECIMAL(18,2);

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @ProductId <= 0
        BEGIN
            RAISERROR('Ürün seçilmelidir.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @Quantity <= 0
        BEGIN
            RAISERROR('Adet sıfırdan büyük olmalıdır.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        SELECT @CurrentStock = Stock
        FROM dbo.Products
        WHERE ProductId = @ProductId
          AND IsActive = 1;

        IF @CurrentStock IS NULL
        BEGIN
            RAISERROR('Ürün bulunamadı veya aktif değil.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @CurrentStock < @Quantity
        BEGIN
            RAISERROR('Seçilen ürün için yeterli stok yok.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @CustomerStoreId IS NOT NULL
        BEGIN
            SELECT
                @CustomerId = c.CustomerId,
                @CustomerName = COALESCE(NULLIF(@CustomerName, ''), NULLIF(c.CompanyName, ''), c.FullName),
                @CustomerEmail = COALESCE(NULLIF(@CustomerEmail, ''), c.Email),
                @CustomerPhone = COALESCE(NULLIF(@CustomerPhone, ''), cs.Phone, c.Phone)
            FROM dbo.CustomerStores cs
            INNER JOIN dbo.Customers c
                ON c.CustomerId = cs.CustomerId
            WHERE cs.CustomerStoreId = @CustomerStoreId
              AND cs.IsActive = 1
              AND c.IsActive = 1;

            IF @CustomerId IS NULL
            BEGIN
                RAISERROR('Seçili bayi/mağaza bulunamadı veya aktif değil.', 16, 1);
                ROLLBACK TRANSACTION;
                RETURN;
            END
        END
        ELSE
        BEGIN
            IF @CustomerName IS NULL OR LTRIM(RTRIM(@CustomerName)) = ''
            BEGIN
                RAISERROR('Müşteri adı boş olamaz.', 16, 1);
                ROLLBACK TRANSACTION;
                RETURN;
            END

            SELECT TOP 1 @CustomerId = CustomerId
            FROM dbo.Customers
            WHERE FullName = @CustomerName
               OR CompanyName = @CustomerName
            ORDER BY CustomerId;

            IF @CustomerId IS NULL
            BEGIN
                INSERT INTO dbo.Customers
                (
                    FullName,
                    CompanyName,
                    Phone,
                    Email,
                    CustomerType,
                    IsActive
                )
                VALUES
                (
                    @CustomerName,
                    @CustomerName,
                    @CustomerPhone,
                    @CustomerEmail,
                    N'Toptan',
                    1
                );

                SET @CustomerId = SCOPE_IDENTITY();
            END

            SELECT TOP 1 @CustomerStoreId = CustomerStoreId
            FROM dbo.CustomerStores
            WHERE CustomerId = @CustomerId
            ORDER BY CustomerStoreId;

            IF @CustomerStoreId IS NULL
            BEGIN
                INSERT INTO dbo.CustomerStores
                (
                    CustomerId,
                    StoreName,
                    Phone,
                    IsActive
                )
                VALUES
                (
                    @CustomerId,
                    @CustomerName + N' Ana Mağaza',
                    @CustomerPhone,
                    1
                );

                SET @CustomerStoreId = SCOPE_IDENTITY();
            END
        END

        SET @UnitPrice = CASE
                            WHEN @Quantity > 0 THEN @TotalPrice / @Quantity
                            ELSE @TotalPrice
                         END;

        IF @BayiYetkiliId IS NOT NULL
        BEGIN
            IF NOT EXISTS
            (
                SELECT 1
                FROM dbo.BayiYetkilileri byk
                WHERE byk.BayiYetkiliId = @BayiYetkiliId
                  AND byk.BayiId = @CustomerId
                  AND byk.AktifMi = 1
                  AND (byk.MagazaId IS NULL OR byk.MagazaId = @CustomerStoreId)
            )
            BEGIN
                RAISERROR('Seçili bayi yetkilisi bu bayi/mağaza için geçerli değil.', 16, 1);
                ROLLBACK TRANSACTION;
                RETURN;
            END
        END

        INSERT INTO dbo.Orders
        (
            CustomerName,
            CustomerEmail,
            CustomerPhone,
            ProductId,
            Quantity,
            TotalPrice,
            OrderStatus,
            CustomerId,
            CustomerStoreId,
            OrderNo,
            OrderType,
            OrderSource,
            PaymentStatus,
            SubTotal,
            DiscountTotal,
            ShippingTotal,
            GrandTotal,
            IsCancelled,
            IsArchived,
            BayiYetkiliId
        )
        VALUES
        (
            @CustomerName,
            @CustomerEmail,
            @CustomerPhone,
            @ProductId,
            @Quantity,
            @TotalPrice,
            N'Hazirlaniyor',
            @CustomerId,
            @CustomerStoreId,
            NULL,
            @OrderType,
            @OrderSource,
            N'Bekliyor',
            @TotalPrice,
            0,
            0,
            @TotalPrice,
            0,
            0,
            @BayiYetkiliId
        );

        SET @OrderId = SCOPE_IDENTITY();

        UPDATE dbo.Orders
        SET OrderNo = 'ORD-' + RIGHT('000000' + CAST(OrderId AS NVARCHAR(20)), 6)
        WHERE OrderId = @OrderId;

        INSERT INTO dbo.OrderItems
        (
            OrderId,
            ProductId,
            Quantity,
            UnitPrice,
            DiscountRate,
            DiscountAmount,
            LineTotal
        )
        VALUES
        (
            @OrderId,
            @ProductId,
            @Quantity,
            @UnitPrice,
            0,
            0,
            @TotalPrice
        );

        UPDATE dbo.Products
        SET Stock = Stock - @Quantity
        WHERE ProductId = @ProductId;

        INSERT INTO dbo.StockMovements
        (
            ProductId,
            OrderId,
            MovementType,
            Quantity,
            Description
        )
        VALUES
        (
            @ProductId,
            @OrderId,
            N'OrderOut',
            @Quantity,
            @OrderSource + N' üzerinden sipariş oluşturuldu.'
        );

        COMMIT TRANSACTION;

        SELECT @OrderId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000);
        DECLARE @ErrorSeverity INT;
        DECLARE @ErrorState INT;

        SELECT
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO
