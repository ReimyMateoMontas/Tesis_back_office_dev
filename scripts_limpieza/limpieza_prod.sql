-- =============================================================================
--  LIMPIEZA DE DATOS - BASE DE DATOS EDEN (ENTORNO PRODUCCIÓN)
--  Generado: 2026-08-21
--
--  Contenido:
--    1) Renombrado de zonas (mantiene ID / trazabilidad, solo cambia el nombre)
--    2) Alta de nuevas zonas (Área de gatos, Área verde, Área amarilla, Área abierta)
--    3) Borrado de usuarios Veterinario/Trabajador SIN referencias (solo los libres)
--
--  IMPORTANTE:
--    - Ejecutar SOLO después de haber validado en local con limpieza_local.sql
--    - Haz un backup antes de ejecutar.
--    - El script va dentro de una transacción. Revisa los SELECT de verificación
--      del final ANTES de hacer COMMIT. Cambia COMMIT por ROLLBACK si algo no cuadra.
-- =============================================================================

SET NAMES utf8mb4;
SET SESSION sql_mode = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION';

-- Esquema objetivo en PRODUCCIÓN (Railway). El dump y la API usan la base `backupeden`.
-- IMPORTANTE: en Workbench, al conectarte a Railway, la base por defecto suele ser
-- `railway` (vacía). Este USE fuerza el esquema correcto. Si tu base productiva tiene
-- otro nombre, cámbialo aquí.
USE `backupeden`;

START TRANSACTION;

-- -----------------------------------------------------------------------------
-- 1) RENOMBRAR ZONAS EXISTENTES (solo el nombre; mismo Id, capacidad y cantidad)
--    Se emparejan por Id + nombre actual para que sea idempotente y seguro.
-- -----------------------------------------------------------------------------
UPDATE `zonas` SET `nombre` = 'Área azul'    WHERE `Id` = 1 AND `nombre` = 'Zona azul';
UPDATE `zonas` SET `nombre` = 'Área clínica' WHERE `Id` = 3 AND `nombre` = 'Médica';
UPDATE `zonas` SET `nombre` = 'Área rosada'  WHERE `Id` = 6 AND `nombre` = 'Zona roja';
-- Se mantienen sin cambios: Id 2 'Adopción', Id 4 'Cachorros', Id 5 'Rehabilitación'

-- -----------------------------------------------------------------------------
-- 2) NUEVAS ZONAS
--    capacidad_maxima es NOT NULL: se ponen valores por defecto razonables.
--    >>> AJUSTA capacidad_maxima / descripcion a los valores reales si aplica <<<
-- -----------------------------------------------------------------------------
INSERT INTO `zonas` (`nombre`, `descripcion`, `capacidad_maxima`, `cantidad_actual`, `activa`)
SELECT * FROM (
    SELECT 'Área de gatos' AS n, 'Zona destinada a gatos' AS d, 20 AS c, 0 AS a, 1 AS act
    UNION ALL SELECT 'Área verde',   'Zona verde', 30, 0, 1
    UNION ALL SELECT 'Área amarilla','Zona amarilla', 30, 0, 1
    UNION ALL SELECT 'Área abierta', 'Zona abierta: incluye Banco y patio al fondo, área de adopción y zonas de cachorros', 100, 0, 1
) AS nuevas
WHERE NOT EXISTS (SELECT 1 FROM `zonas` z WHERE z.`nombre` = nuevas.n);

-- -----------------------------------------------------------------------------
-- 3) BORRAR USUARIOS Veterinario/Trabajador QUE NO ESTÉN REFERENCIADOS
--    (los referenciados por FK RESTRICT se conservan para no romper trazabilidad)
--
--    Esperado en PRODUCCIÓN -> se borran ids: 8, 10, 12, 13, 18, 20, 21, 24
--    Se conservan por tener referencias: 3, 7  (id 17 en prod es Administrador, no se toca)
--
--    La condición NOT IN se calcula sobre TODAS las columnas que apuntan a usuarios,
--    filtrando IS NOT NULL (evita el problema de NOT IN con NULL). Es auto-ajustable:
--    si los datos cambiaron desde el backup, solo borrará los realmente libres.
-- -----------------------------------------------------------------------------
DELETE FROM `usuarios`
WHERE `rol` IN ('Veterinario','Trabajador')
  AND `Id` NOT IN (SELECT `usuario_responsable_id` FROM `adopciones`            WHERE `usuario_responsable_id` IS NOT NULL)
  AND `Id` NOT IN (SELECT `usuario_registro_id`     FROM `animales`             WHERE `usuario_registro_id`     IS NOT NULL)
  AND `Id` NOT IN (SELECT `UsuarioCierreId`         FROM `cierresmensuales`     WHERE `UsuarioCierreId`         IS NOT NULL)
  AND `Id` NOT IN (SELECT `UsuarioRegistroId`       FROM `donaciones`           WHERE `UsuarioRegistroId`       IS NOT NULL)
  AND `Id` NOT IN (SELECT `usuario_responsable_id`  FROM `estadosgenerales`     WHERE `usuario_responsable_id`  IS NOT NULL)
  AND `Id` NOT IN (SELECT `veterinario_id`          FROM `fallecimientos`       WHERE `veterinario_id`          IS NOT NULL)
  AND `Id` NOT IN (SELECT `usuario_registro_id`     FROM `fallecimientos`       WHERE `usuario_registro_id`     IS NOT NULL)
  AND `Id` NOT IN (SELECT `UsuarioRegistroId`       FROM `gastos`               WHERE `UsuarioRegistroId`       IS NOT NULL)
  AND `Id` NOT IN (SELECT `veterinario_id`          FROM `historialmedico`      WHERE `veterinario_id`          IS NOT NULL)
  AND `Id` NOT IN (SELECT `usuario_responsable_id`  FROM `historialmovimientos` WHERE `usuario_responsable_id`  IS NOT NULL)
  AND `Id` NOT IN (SELECT `usuario_responsable_id`  FROM `movimientosinventario`WHERE `usuario_responsable_id`  IS NOT NULL)
  AND `Id` NOT IN (SELECT `UsuarioCreoId`           FROM `objetivos`            WHERE `UsuarioCreoId`           IS NOT NULL)
  AND `Id` NOT IN (SELECT `usuario_responsable_id`  FROM `transferencias`       WHERE `usuario_responsable_id`  IS NOT NULL)
  AND `Id` NOT IN (SELECT `veterinario_id`          FROM `tratamientos`         WHERE `veterinario_id`          IS NOT NULL)
  AND `Id` NOT IN (SELECT `veterinario_id`          FROM `vacunas`              WHERE `veterinario_id`          IS NOT NULL);

-- -----------------------------------------------------------------------------
-- VERIFICACIÓN (revisar antes de COMMIT)
-- -----------------------------------------------------------------------------
SELECT '--- ZONAS (estado final) ---' AS info;
SELECT `Id`, `nombre`, `capacidad_maxima`, `cantidad_actual`, `activa` FROM `zonas` ORDER BY `Id`;

SELECT '--- USUARIOS por rol (deben quedar 0 Vet/Trab LIBRES; los referenciados siguen) ---' AS info;
SELECT `rol`, COUNT(*) AS total FROM `usuarios` GROUP BY `rol`;

SELECT '--- Vet/Trab que quedan (solo deberían ser los referenciados: 3, 7) ---' AS info;
SELECT `Id`, `nombre`, `apellido`, `email`, `rol` FROM `usuarios`
WHERE `rol` IN ('Veterinario','Trabajador') ORDER BY `Id`;

-- Si todo está correcto:
COMMIT;
-- Si algo no cuadra, usa en su lugar:  ROLLBACK;
