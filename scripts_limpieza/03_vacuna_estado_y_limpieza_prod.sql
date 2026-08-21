-- =============================================================================
--  EDEN — PRODUCCIÓN — Paso 3
--  (1) Feature Vacunas: columnas `estado` y `alerta_enviada`
--  (2) Limpieza de registros basura (historial médico, medicamentos, tratamientos)
--  Generado: 2026-08-21
--
--  Ejecutar SOLO después de validar en local. Mismo esquema `backupeden` en Railway.
--  Requiere el backend actualizado (Vacuna.Estado) para usar la columna nueva,
--  pero el ALTER puede correrse antes sin romper nada (la columna tiene default).
-- =============================================================================
SET NAMES utf8mb4;
SET SESSION sql_mode = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION';

-- La API usa la base `backupeden` (ver appsettings.json). NO usar `railway`.
USE `backupeden`;

START TRANSACTION;

-- -----------------------------------------------------------------------------
-- 1) SCHEMA: agregar columnas a `vacunas` (idempotente; MySQL no soporta
--    ADD COLUMN IF NOT EXISTS, así que se verifica en information_schema)
-- -----------------------------------------------------------------------------
SET @col_estado := (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'vacunas' AND COLUMN_NAME = 'estado');
SET @sql := IF(@col_estado = 0,
    "ALTER TABLE `vacunas` ADD COLUMN `estado` enum('Pendiente','Completada') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Pendiente' AFTER `observaciones`",
    "SELECT 'columna estado ya existe' AS info");
PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;

SET @col_alerta := (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'vacunas' AND COLUMN_NAME = 'alerta_enviada');
SET @sql := IF(@col_alerta = 0,
    "ALTER TABLE `vacunas` ADD COLUMN `alerta_enviada` date DEFAULT NULL AFTER `estado`",
    "SELECT 'columna alerta_enviada ya existe' AS info");
PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;

-- Índice para consultas de alertas (proxima_dosis + estado). Idempotente.
SET @idx := (SELECT COUNT(*) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'vacunas' AND INDEX_NAME = 'idx_vacunas_estado');
SET @sql := IF(@idx = 0,
    "ALTER TABLE `vacunas` ADD INDEX `idx_vacunas_estado` (`estado`)",
    "SELECT 'indice idx_vacunas_estado ya existe' AS info");
PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;

-- -----------------------------------------------------------------------------
-- 2) BACKFILL de estado:
--    - Las vacunas cuyo proxima_dosis YA pasó (históricas) se marcan 'Completada'
--      para no generar una avalancha de alertas viejas al activar la feature.
--    - Las que tienen proxima_dosis de hoy en adelante quedan 'Pendiente'.
--    Solo aplica una vez (cuando la columna recién se crea / sigue en default).
-- -----------------------------------------------------------------------------
UPDATE `vacunas`
   SET `estado` = 'Completada'
 WHERE `estado` = 'Pendiente'
   AND `proxima_dosis` IS NOT NULL
   AND `proxima_dosis` < CURRENT_DATE();

-- -----------------------------------------------------------------------------
-- 3) LIMPIEZA DE DATOS BASURA  (match por Id + valor viejo = seguro e idempotente)
-- -----------------------------------------------------------------------------
-- Historial médico (diagnósticos/síntomas inventados y bromas)
UPDATE `historialmedico` SET `diagnostico`='Otitis externa',            `sintomas`='Sacudidas de cabeza y mal olor del oído'      WHERE `Id`=26 AND `diagnostico`='wqwewq';
UPDATE `historialmedico` SET `diagnostico`='Gastroenteritis leve',      `sintomas`='Vómitos y diarrea', `observaciones`='Dieta blanda por 3 días'  WHERE `Id`=27 AND `diagnostico`='213212';
UPDATE `historialmedico` SET `diagnostico`='Chequeo general'                                                              WHERE `Id`=30 AND `diagnostico`='eewr';
UPDATE `historialmedico` SET `diagnostico`='Conjuntivitis',             `sintomas`='Secreción ocular y enrojecimiento'           WHERE `Id`=34 AND `diagnostico`='ssasaads';
UPDATE `historialmedico` SET `diagnostico`='Desparasitación de rutina'                                                    WHERE `Id`=35 AND `diagnostico`='jhsjwhks';
UPDATE `historialmedico` SET `diagnostico`='Dermatitis',               `sintomas`='Enrojecimiento y picazón en la piel', `observaciones`='Se indica baño medicado'  WHERE `Id`=36 AND `diagnostico`='Rabia ';
UPDATE `historialmedico` SET `diagnostico`='Control post-vacunación',   `sintomas`='Sin síntomas', `observaciones`='Animal estable'  WHERE `Id`=37 AND `diagnostico`='Rabia';
UPDATE `historialmedico` SET `diagnostico`='Infección respiratoria leve',`sintomas`='Tos y secreción nasal'                     WHERE `Id`=39 AND `diagnostico`='dsasd';
UPDATE `historialmedico` SET `diagnostico`='Chequeo de rutina',        `sintomas`='Ninguno'                                     WHERE `Id`=46 AND `diagnostico`='prueba';
UPDATE `historialmedico` SET `diagnostico`='Control de peso',          `sintomas`='Ninguno'                                     WHERE `Id`=47 AND `diagnostico`='Prueba';
UPDATE `historialmedico` SET `diagnostico`='Infección bacteriana',     `sintomas`='Fiebre y decaimiento leve'                   WHERE `Id`=48 AND `diagnostico`='qweeq';
-- Observaciones basura sueltas
UPDATE `historialmedico` SET `observaciones`=NULL WHERE `Id` IN (28,29) AND `observaciones`='2';

-- Medicamentos inventados / con typo
UPDATE `medicamentos` SET `nombre`='Dexametasona'        WHERE `Id`=6 AND `nombre`='SADSAS';
UPDATE `medicamentos` SET `nombre`='Amoxicilina 250 mg'  WHERE `Id`=8 AND `nombre`='Amoxicilin';

-- Tratamientos con dosis/frecuencia inventadas
UPDATE `tratamientos` SET `dosis`='250 mg',   `frecuencia`='Cada 12 horas' WHERE `Id`=6 AND `dosis`='23'  AND `frecuencia`='2132';
UPDATE `tratamientos` SET `dosis`='1 tableta',`frecuencia`='Cada 24 horas' WHERE `Id`=7 AND `dosis`='2'   AND `frecuencia`='Diarias';
UPDATE `tratamientos` SET `dosis`='1 tableta',`frecuencia`='Cada 12 horas' WHERE `Id`=8 AND `dosis`='2'   AND `frecuencia`='12 hora';

-- -----------------------------------------------------------------------------
-- VERIFICACIÓN (revisar antes de COMMIT)
-- -----------------------------------------------------------------------------
SELECT '--- vacunas: estructura estado/alerta ---' AS info;
SELECT `Id`, `animal_id`, `proxima_dosis`, `estado`, `alerta_enviada` FROM `vacunas` ORDER BY `Id`;

SELECT '--- historial médico limpiado (26,27,30,34,35,36,37,39,46,47,48) ---' AS info;
SELECT `Id`, `diagnostico`, `sintomas`, `observaciones` FROM `historialmedico`
WHERE `Id` IN (26,27,30,34,35,36,37,39,46,47,48) ORDER BY `Id`;

SELECT '--- medicamentos 6 y 8 ---' AS info;
SELECT `Id`, `nombre` FROM `medicamentos` WHERE `Id` IN (6,8);

SELECT '--- tratamientos 6,7,8 ---' AS info;
SELECT `Id`, `dosis`, `frecuencia` FROM `tratamientos` WHERE `Id` IN (6,7,8);

-- Si todo está correcto:
COMMIT;
-- Si algo no cuadra:  ROLLBACK;
