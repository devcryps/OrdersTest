-- Esquema Oracle para OrdersManagement (compatible con Oracle 12c o superior por el uso de OFFSET/FETCH).
-- Ejecutar con el usuario/esquema dueño de las tablas.

CREATE SEQUENCE SEQ_ORDERS START WITH 1 INCREMENT BY 1 NOCACHE;

CREATE SEQUENCE SEQ_ORDERITEMS START WITH 1 INCREMENT BY 1 NOCACHE;

CREATE TABLE Orders (
    Id         NUMBER(19)     NOT NULL,
    Customer   VARCHAR2(200)  NOT NULL,
    Total      NUMBER(18, 2)  NOT NULL,
    Status     VARCHAR2(20)   NOT NULL,
    CreatedAt  TIMESTAMP      NOT NULL,
    CONSTRAINT PK_Orders PRIMARY KEY (Id),
    CONSTRAINT CK_Orders_Total CHECK (Total > 0),
    CONSTRAINT CK_Orders_Status CHECK (Status IN ('Pending', 'Confirmed'))
);

CREATE TABLE OrderItems (
    Id           NUMBER(19)     NOT NULL,
    OrderId      NUMBER(19)     NOT NULL,
    Description  VARCHAR2(500)  NOT NULL,
    Quantity     NUMBER(10)     NOT NULL,
    UnitPrice    NUMBER(18, 2)  NOT NULL,
    CONSTRAINT PK_OrderItems PRIMARY KEY (Id),
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders (Id),
    CONSTRAINT CK_OrderItems_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_OrderItems_UnitPrice CHECK (UnitPrice >= 0)
);

-- Soporta la consulta de ítems por pedido y el orden del listado paginado.
CREATE INDEX IX_OrderItems_OrderId ON OrderItems (OrderId);

CREATE INDEX IX_Orders_CreatedAt_Id ON Orders (CreatedAt DESC, Id DESC);
