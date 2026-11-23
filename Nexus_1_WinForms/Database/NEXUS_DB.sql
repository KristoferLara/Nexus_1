/*
    NEXUS_1 - Base de datos SQL Server
    Rama objetivo: nexus_ani
    Proyecto: Nexus_1_WinForms
    Ejecutar este archivo completo desde SQL Server Management Studio (SSMS).

    El script es idempotente: crea la base, tablas, indices y catalogos
    solamente cuando no existen. No elimina ni sobrescribe partidas.
*/
USE master;
GO

IF DB_ID(N'NEXUS_DB') IS NULL
BEGIN
    CREATE DATABASE NEXUS_DB;
END;
GO

USE NEXUS_DB;
GO

/* Catalogo de clases disponibles en la pantalla de seleccion. */
IF OBJECT_ID(N'dbo.ClasesCadete', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClasesCadete
    (
        ClaseId       TINYINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ClasesCadete PRIMARY KEY,
        Codigo        VARCHAR(20) NOT NULL CONSTRAINT UQ_ClasesCadete_Codigo UNIQUE,
        Nombre        NVARCHAR(60) NOT NULL,
        Descripcion   NVARCHAR(250) NOT NULL,
        IniciaConArma BIT NOT NULL CONSTRAINT DF_ClasesCadete_IniciaConArma DEFAULT (0),
        Activa        BIT NOT NULL CONSTRAINT DF_ClasesCadete_Activa DEFAULT (1)
    );
END;
GO

/* Catalogo de tipos de enemigo usados por las misiones. */
IF OBJECT_ID(N'dbo.TiposEnemigo', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TiposEnemigo
    (
        TipoEnemigoId TINYINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TiposEnemigo PRIMARY KEY,
        Codigo        VARCHAR(20) NOT NULL CONSTRAINT UQ_TiposEnemigo_Codigo UNIQUE,
        Nombre        NVARCHAR(60) NOT NULL,
        VidaMaxima    SMALLINT NOT NULL CONSTRAINT DF_TiposEnemigo_Vida DEFAULT (25),
        Activo        BIT NOT NULL CONSTRAINT DF_TiposEnemigo_Activo DEFAULT (1),
        CONSTRAINT CK_TiposEnemigo_Vida CHECK (VidaMaxima > 0)
    );
END;
GO

/* Catalogo de misiones: sus datos son comunes a todas las partidas. */
IF OBJECT_ID(N'dbo.Misiones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Misiones
    (
        MisionId          SMALLINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Misiones PRIMARY KEY,
        Codigo            VARCHAR(30) NOT NULL CONSTRAINT UQ_Misiones_Codigo UNIQUE,
        Nombre            NVARCHAR(100) NOT NULL,
        Descripcion       NVARCHAR(300) NOT NULL,
        TipoEnemigoId     TINYINT NOT NULL,
        Consejo           NVARCHAR(500) NOT NULL,
        RecompensaPoder   SMALLINT NOT NULL,
        RecompensaXP      INT NOT NULL,
        Orden             TINYINT NOT NULL,
        Activa            BIT NOT NULL CONSTRAINT DF_Misiones_Activa DEFAULT (1),
        CONSTRAINT FK_Misiones_TiposEnemigo FOREIGN KEY (TipoEnemigoId)
            REFERENCES dbo.TiposEnemigo(TipoEnemigoId),
        CONSTRAINT UQ_Misiones_Orden UNIQUE (Orden),
        CONSTRAINT CK_Misiones_Recompensas CHECK (RecompensaPoder >= 0 AND RecompensaXP >= 0)
    );
END;
GO

/* Perfil del cadete. Una persona puede tener varias partidas guardadas. */
IF OBJECT_ID(N'dbo.Cadetes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cadetes
    (
        CadeteId      BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Cadetes PRIMARY KEY,
        Nombre        NVARCHAR(20) NOT NULL,
        FechaRegistro DATETIME2(0) NOT NULL CONSTRAINT DF_Cadetes_Fecha DEFAULT (SYSDATETIME()),
        Activo        BIT NOT NULL CONSTRAINT DF_Cadetes_Activo DEFAULT (1),
        CONSTRAINT CK_Cadetes_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) BETWEEN 1 AND 20)
    );
END;
GO

/* Una fila representa una partida y conserva el estado necesario para retomarla. */
IF OBJECT_ID(N'dbo.Partidas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Partidas
    (
        PartidaId          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Partidas PRIMARY KEY,
        CadeteId           BIGINT NOT NULL,
        ClaseId            TINYINT NOT NULL,
        Estado             VARCHAR(15) NOT NULL CONSTRAINT DF_Partidas_Estado DEFAULT ('ACTIVA'),
        FechaInicio        DATETIME2(0) NOT NULL CONSTRAINT DF_Partidas_Inicio DEFAULT (SYSDATETIME()),
        FechaActualizacion DATETIME2(0) NOT NULL CONSTRAINT DF_Partidas_Actualizacion DEFAULT (SYSDATETIME()),
        FechaFin           DATETIME2(0) NULL,

        -- Estadisticas actuales del objeto Cadete.
        Energia            TINYINT NOT NULL CONSTRAINT DF_Partidas_Energia DEFAULT (100),
        Estabilidad        TINYINT NOT NULL CONSTRAINT DF_Partidas_Estabilidad DEFAULT (100),
        Vida               TINYINT NOT NULL CONSTRAINT DF_Partidas_Vida DEFAULT (100),
        Poder              TINYINT NOT NULL CONSTRAINT DF_Partidas_Poder DEFAULT (0),
        Experiencia        INT NOT NULL CONSTRAINT DF_Partidas_Experiencia DEFAULT (0),
        TurnosCapacidad    TINYINT NOT NULL CONSTRAINT DF_Partidas_Turnos DEFAULT (0),
        TieneArma          BIT NOT NULL CONSTRAINT DF_Partidas_Arma DEFAULT (0),

        -- Progreso y ubicacion en los mapas.
        MisionIndice       TINYINT NOT NULL CONSTRAINT DF_Partidas_MisionIndice DEFAULT (0),
        Distancia          INT NOT NULL CONSTRAINT DF_Partidas_Distancia DEFAULT (0),
        BaseFila           SMALLINT NOT NULL CONSTRAINT DF_Partidas_BaseFila DEFAULT (2),
        BaseColumna        SMALLINT NOT NULL CONSTRAINT DF_Partidas_BaseCol DEFAULT (2),
        GenesisFila        SMALLINT NOT NULL CONSTRAINT DF_Partidas_GenesisFila DEFAULT (0),
        GenesisColumna     SMALLINT NOT NULL CONSTRAINT DF_Partidas_GenesisCol DEFAULT (0),
        Tick               BIGINT NOT NULL CONSTRAINT DF_Partidas_Tick DEFAULT (0),

        CONSTRAINT FK_Partidas_Cadetes FOREIGN KEY (CadeteId) REFERENCES dbo.Cadetes(CadeteId),
        CONSTRAINT FK_Partidas_Clases FOREIGN KEY (ClaseId) REFERENCES dbo.ClasesCadete(ClaseId),
        CONSTRAINT CK_Partidas_Estado CHECK (Estado IN ('ACTIVA','PAUSADA','FINALIZADA','DERROTA')),
        CONSTRAINT CK_Partidas_Energia CHECK (Energia BETWEEN 0 AND 100),
        CONSTRAINT CK_Partidas_Estabilidad CHECK (Estabilidad BETWEEN 0 AND 100),
        CONSTRAINT CK_Partidas_Vida CHECK (Vida BETWEEN 0 AND 100),
        CONSTRAINT CK_Partidas_Poder CHECK (Poder BETWEEN 0 AND 100),
        CONSTRAINT CK_Partidas_Experiencia CHECK (Experiencia >= 0),
        CONSTRAINT CK_Partidas_Turnos CHECK (TurnosCapacidad BETWEEN 0 AND 3),
        CONSTRAINT CK_Partidas_Progreso CHECK (MisionIndice BETWEEN 0 AND 3 AND Distancia >= 0 AND Tick >= 0)
    );
END;
GO

/* Estado individual de cada mision dentro de una partida. */
IF OBJECT_ID(N'dbo.ProgresoMisiones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProgresoMisiones
    (
        ProgresoMisionId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProgresoMisiones PRIMARY KEY,
        PartidaId        BIGINT NOT NULL,
        MisionId         SMALLINT NOT NULL,
        ObjetivoCumplido BIT NOT NULL CONSTRAINT DF_Progreso_Objetivo DEFAULT (0),
        Completada       BIT NOT NULL CONSTRAINT DF_Progreso_Completada DEFAULT (0),
        FechaObjetivo    DATETIME2(0) NULL,
        FechaCompletada  DATETIME2(0) NULL,
        CONSTRAINT FK_Progreso_Partidas FOREIGN KEY (PartidaId) REFERENCES dbo.Partidas(PartidaId),
        CONSTRAINT FK_Progreso_Misiones FOREIGN KEY (MisionId) REFERENCES dbo.Misiones(MisionId),
        CONSTRAINT UQ_Progreso_PartidaMision UNIQUE (PartidaId, MisionId),
        CONSTRAINT CK_Progreso_Fechas CHECK (Completada = 0 OR ObjetivoCumplido = 1)
    );
END;
GO

/* Instancias de enemigos colocados en el mapa Genesis para cada partida. */
IF OBJECT_ID(N'dbo.EnemigosPartida', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EnemigosPartida
    (
        EnemigoPartidaId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EnemigosPartida PRIMARY KEY,
        PartidaId        BIGINT NOT NULL,
        TipoEnemigoId    TINYINT NOT NULL,
        Fila             SMALLINT NOT NULL,
        Columna          SMALLINT NOT NULL,
        VidaActual       SMALLINT NOT NULL,
        Derrotado        BIT NOT NULL CONSTRAINT DF_Enemigos_Derrotado DEFAULT (0),
        FechaDerrota     DATETIME2(0) NULL,
        CONSTRAINT FK_Enemigos_Partidas FOREIGN KEY (PartidaId) REFERENCES dbo.Partidas(PartidaId),
        CONSTRAINT FK_Enemigos_Tipos FOREIGN KEY (TipoEnemigoId) REFERENCES dbo.TiposEnemigo(TipoEnemigoId),
        CONSTRAINT UQ_Enemigos_Posicion UNIQUE (PartidaId, Fila, Columna),
        CONSTRAINT CK_Enemigos_Vida CHECK (VidaActual >= 0),
        CONSTRAINT CK_Enemigos_Derrota CHECK ((Derrotado = 0 AND VidaActual > 0) OR (Derrotado = 1 AND VidaActual = 0))
    );
END;
GO

/* Catalogo de objetos recolectables y recursos del inventario. */
IF OBJECT_ID(N'dbo.Items', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Items
    (
        ItemId       SMALLINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Items PRIMARY KEY,
        Codigo       VARCHAR(30) NOT NULL CONSTRAINT UQ_Items_Codigo UNIQUE,
        Nombre       NVARCHAR(80) NOT NULL,
        Descripcion  NVARCHAR(250) NOT NULL,
        Tipo         VARCHAR(20) NOT NULL,
        Activo       BIT NOT NULL CONSTRAINT DF_Items_Activo DEFAULT (1),
        CONSTRAINT CK_Items_Tipo CHECK (Tipo IN ('RECURSO','CONSUMIBLE','EQUIPAMIENTO'))
    );
END;
GO

IF OBJECT_ID(N'dbo.Inventario', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Inventario
    (
        InventarioId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inventario PRIMARY KEY,
        PartidaId    BIGINT NOT NULL,
        ItemId       SMALLINT NOT NULL,
        Cantidad     INT NOT NULL CONSTRAINT DF_Inventario_Cantidad DEFAULT (0),
        CONSTRAINT FK_Inventario_Partidas FOREIGN KEY (PartidaId) REFERENCES dbo.Partidas(PartidaId),
        CONSTRAINT FK_Inventario_Items FOREIGN KEY (ItemId) REFERENCES dbo.Items(ItemId),
        CONSTRAINT UQ_Inventario_PartidaItem UNIQUE (PartidaId, ItemId),
        CONSTRAINT CK_Inventario_Cantidad CHECK (Cantidad >= 0)
    );
END;
GO

/* Celdas visitadas/reveladas y botiquines ya recogidos en Genesis. */
IF OBJECT_ID(N'dbo.ExploracionCeldas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExploracionCeldas
    (
        ExploracionId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ExploracionCeldas PRIMARY KEY,
        PartidaId     BIGINT NOT NULL,
        Fila          SMALLINT NOT NULL,
        Columna       SMALLINT NOT NULL,
        Explorada     BIT NOT NULL CONSTRAINT DF_Exploracion_Explorada DEFAULT (1),
        RecursoRecogido BIT NOT NULL CONSTRAINT DF_Exploracion_Recurso DEFAULT (0),
        FechaVisita   DATETIME2(0) NOT NULL CONSTRAINT DF_Exploracion_Fecha DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_Exploracion_Partidas FOREIGN KEY (PartidaId) REFERENCES dbo.Partidas(PartidaId),
        CONSTRAINT UQ_Exploracion_Celda UNIQUE (PartidaId, Fila, Columna)
    );
END;
GO

/* Registro historico de cada encuentro de combate. */
IF OBJECT_ID(N'dbo.HistorialCombates', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HistorialCombates
    (
        CombateId          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HistorialCombates PRIMARY KEY,
        PartidaId          BIGINT NOT NULL,
        EnemigoPartidaId   BIGINT NOT NULL,
        FechaInicio        DATETIME2(0) NOT NULL CONSTRAINT DF_Combates_Inicio DEFAULT (SYSDATETIME()),
        FechaFin           DATETIME2(0) NULL,
        DanioTotalJugador  INT NOT NULL CONSTRAINT DF_Combates_DanioJugador DEFAULT (0),
        DanioTotalEnemigo  INT NOT NULL CONSTRAINT DF_Combates_DanioEnemigo DEFAULT (0),
        Resultado          VARCHAR(12) NOT NULL CONSTRAINT DF_Combates_Resultado DEFAULT ('EN_CURSO'),
        Huyo               BIT NOT NULL CONSTRAINT DF_Combates_Huyo DEFAULT (0),
        EraObjetivoMision  BIT NOT NULL CONSTRAINT DF_Combates_Objetivo DEFAULT (0),
        CONSTRAINT FK_Combates_Partidas FOREIGN KEY (PartidaId) REFERENCES dbo.Partidas(PartidaId),
        CONSTRAINT FK_Combates_Enemigos FOREIGN KEY (EnemigoPartidaId) REFERENCES dbo.EnemigosPartida(EnemigoPartidaId),
        CONSTRAINT CK_Combates_Danios CHECK (DanioTotalJugador >= 0 AND DanioTotalEnemigo >= 0),
        CONSTRAINT CK_Combates_Resultado CHECK (Resultado IN ('EN_CURSO','VICTORIA','DERROTA','HUIDA'))
    );
END;
GO

/* Bitacora funcional para guardar acciones importantes y facilitar depuracion. */
IF OBJECT_ID(N'dbo.EventosPartida', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventosPartida
    (
        EventoId    BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EventosPartida PRIMARY KEY,
        PartidaId   BIGINT NOT NULL,
        TipoEvento  VARCHAR(30) NOT NULL,
        Descripcion NVARCHAR(500) NOT NULL,
        Fecha       DATETIME2(0) NOT NULL CONSTRAINT DF_Eventos_Fecha DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_Eventos_Partidas FOREIGN KEY (PartidaId) REFERENCES dbo.Partidas(PartidaId)
    );
END;
GO

/* Indices para consultas habituales de guardado, misiones e historial. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Partidas_Cadete_Estado' AND object_id = OBJECT_ID(N'dbo.Partidas'))
    CREATE INDEX IX_Partidas_Cadete_Estado ON dbo.Partidas(CadeteId, Estado, FechaActualizacion DESC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_EnemigosPartida_Partida' AND object_id = OBJECT_ID(N'dbo.EnemigosPartida'))
    CREATE INDEX IX_EnemigosPartida_Partida ON dbo.EnemigosPartida(PartidaId, Derrotado);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_HistorialCombates_Partida_Fecha' AND object_id = OBJECT_ID(N'dbo.HistorialCombates'))
    CREATE INDEX IX_HistorialCombates_Partida_Fecha ON dbo.HistorialCombates(PartidaId, FechaInicio DESC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_EventosPartida_Partida_Fecha' AND object_id = OBJECT_ID(N'dbo.EventosPartida'))
    CREATE INDEX IX_EventosPartida_Partida_Fecha ON dbo.EventosPartida(PartidaId, Fecha DESC);
GO

/* Datos iniciales basados en Juego.cs y Modelos.cs de nexus_ani. */
IF NOT EXISTS (SELECT 1 FROM dbo.ClasesCadete)
BEGIN
    INSERT INTO dbo.ClasesCadete (Codigo, Nombre, Descripcion, IniciaConArma)
    VALUES
      ('FISICO', N'Físico', N'Especialista en combate cuerpo a cuerpo', 0),
      ('ARMAMENTO', N'Armamento', N'Especialista en combate con armas', 1),
      ('MANA', N'Mana', N'Especialista en energía elemental', 0),
      ('HERRAMIENTAS', N'Herramientas', N'Especialista tecnológico', 0);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.TiposEnemigo)
BEGIN
    INSERT INTO dbo.TiposEnemigo (Codigo, Nombre, VidaMaxima)
    VALUES ('LIQUIDO', N'Líquido', 25), ('FUEGO', N'Fuego', 25), ('VENENO', N'Veneno', 25);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Misiones)
BEGIN
    INSERT INTO dbo.Misiones
        (Codigo, Nombre, Descripcion, TipoEnemigoId, Consejo, RecompensaPoder, RecompensaXP, Orden)
    SELECT v.Codigo, v.Nombre, v.Descripcion, t.TipoEnemigoId, v.Consejo, v.Poder, v.XP, v.Orden
    FROM (VALUES
        ('MISION_01', N'EL PRIMER OBJETIVO', N'Derrota al enemigo LIQUIDO (6,5).', 'LIQUIDO',
         N'Este enemigo reduce el daño de tus ataques. Ataca rápido antes de que te desgaste.', 20, 50, 1),
        ('MISION_02', N'LA PRUEBA DE FUEGO', N'Derrota al enemigo FUEGO (4,8).', 'FUEGO',
         N'Este enemigo golpea cada vez más fuerte. Cuida tu vida y no alargues el combate.', 20, 50, 2),
        ('MISION_03', N'EL ÚLTIMO VENENO', N'Derrota al enemigo VENENO (2,3).', 'VENENO',
         N'Este enemigo te quita vida en cada turno. Termina el combate lo antes posible.', 20, 50, 3)
    ) AS v(Codigo, Nombre, Descripcion, TipoCodigo, Consejo, Poder, XP, Orden)
    INNER JOIN dbo.TiposEnemigo t ON t.Codigo = v.TipoCodigo;
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Items)
BEGIN
    INSERT INTO dbo.Items (Codigo, Nombre, Descripcion, Tipo)
    VALUES
      ('BOTIQUIN', N'Botiquín', N'Recurso del mapa Genesis que recupera vida.', 'CONSUMIBLE'),
      ('RECURSO_GENESIS', N'Recurso Genesis', N'Recurso recolectado durante la exploración.', 'RECURSO');
END;
GO

/* Vistas de lectura para la futura capa de acceso a datos de Windows Forms. */
CREATE OR ALTER VIEW dbo.vw_PartidasCadete
AS
SELECT p.PartidaId, c.CadeteId, c.Nombre AS NombreCadete,
       cc.Codigo AS Clase, p.Estado, p.FechaInicio, p.FechaActualizacion,
       p.Vida, p.Energia, p.Estabilidad, p.Poder, p.Experiencia,
       (p.Experiencia / 100) + 1 AS Nivel,
       p.MisionIndice, p.Distancia
FROM dbo.Partidas p
INNER JOIN dbo.Cadetes c ON c.CadeteId = p.CadeteId
INNER JOIN dbo.ClasesCadete cc ON cc.ClaseId = p.ClaseId;
GO

CREATE OR ALTER VIEW dbo.vw_ProgresoMisiones
AS
SELECT pm.PartidaId, m.Orden, m.Codigo, m.Nombre, m.Descripcion,
       te.Codigo AS TipoEnemigo, m.RecompensaPoder, m.RecompensaXP,
       pm.ObjetivoCumplido, pm.Completada, pm.FechaObjetivo, pm.FechaCompletada
FROM dbo.ProgresoMisiones pm
INNER JOIN dbo.Misiones m ON m.MisionId = pm.MisionId
INNER JOIN dbo.TiposEnemigo te ON te.TipoEnemigoId = m.TipoEnemigoId;
GO

/* Comprobacion rapida de instalacion y catalogos. */
SELECT N'ClasesCadete' AS Tabla, COUNT(*) AS Registros FROM dbo.ClasesCadete
UNION ALL SELECT N'TiposEnemigo', COUNT(*) FROM dbo.TiposEnemigo
UNION ALL SELECT N'Misiones', COUNT(*) FROM dbo.Misiones
UNION ALL SELECT N'Items', COUNT(*) FROM dbo.Items;
GO
