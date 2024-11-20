SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


-- Base de datos: `atidsuser_rCar`

-- --------------------------------------------------------

-- Estructura de tabla para la tabla `alquilados`

CREATE TABLE `alquilados` (
  `idalquiler` int(11) NOT NULL,
  `idauto` int(11) DEFAULT NULL,
  `idcliente` int(11) DEFAULT NULL,
  `idempleado` int(11) DEFAULT NULL,
  `idfactura` int(11) DEFAULT NULL,
  `fecha` date DEFAULT NULL,
  `fecha_devolver` date DEFAULT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

-- Estructura de tabla para la tabla `autos`

CREATE TABLE `autos` (
  `idauto` int(11) NOT NULL,
  `marca` varchar(20) DEFAULT NULL,
  `modelo` varchar(20) DEFAULT NULL,
  `placa` varchar(20) DEFAULT NULL,
  `tipo` varchar(20) DEFAULT NULL,
  `estado` varchar(20) DEFAULT NULL,
  `costodia` double DEFAULT NULL,
  `imagen` varchar(50) NOT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

-- Estructura de tabla para la tabla `clientes`

CREATE TABLE `clientes` (
  `idcliente` int(11) NOT NULL,
  `nombre` varchar(20) DEFAULT NULL,
  `direccion` varchar(20) DEFAULT NULL,
  `telefono` varchar(20) DEFAULT NULL,
  `dui` varchar(20) DEFAULT NULL,
  `email` varchar(20) DEFAULT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

-- Estructura de tabla para la tabla `empleados`

CREATE TABLE `empleados` (
  `idempleado` int(11) NOT NULL,
  `nombre` varchar(20) DEFAULT NULL,
  `telefono` varchar(20) DEFAULT NULL,
  `cargo` varchar(20) DEFAULT NULL,
  `email` varchar(20) DEFAULT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

-- Estructura de tabla para la tabla `facturas`

CREATE TABLE `facturas` (
  `idfactura` int(11) NOT NULL,
  `idcliente` int(11) DEFAULT NULL,
  `idauto` int(11) DEFAULT NULL,
  `idempleado` int(11) DEFAULT NULL,
  `fecha` date DEFAULT NULL,
  `subtotal` double DEFAULT NULL,
  `iva` double DEFAULT NULL,
  `total` double DEFAULT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- Índices para tablas volcadas

-- Indices de la tabla `alquilados`
ALTER TABLE `alquilados`
  ADD PRIMARY KEY (`idalquiler`);

-- Indices de la tabla `autos`
ALTER TABLE `autos`
  ADD PRIMARY KEY (`idauto`);

-- Indices de la tabla `clientes`
ALTER TABLE `clientes`
  ADD PRIMARY KEY (`idcliente`);

-- Indices de la tabla `empleados`
ALTER TABLE `empleados`
  ADD PRIMARY KEY (`idempleado`);

-- Indices de la tabla `facturas`
ALTER TABLE `facturas`
  ADD PRIMARY KEY (`idfactura`);

-- AUTO_INCREMENT de las tablas volcadas

-- AUTO_INCREMENT de la tabla `alquilados`
ALTER TABLE `alquilados`
  MODIFY `idalquiler` int(11) NOT NULL AUTO_INCREMENT;

-- AUTO_INCREMENT de la tabla `autos`
ALTER TABLE `autos`
  MODIFY `idauto` int(11) NOT NULL AUTO_INCREMENT;

-- AUTO_INCREMENT de la tabla `clientes`
ALTER TABLE `clientes`
  MODIFY `idcliente` int(11) NOT NULL AUTO_INCREMENT;

-- AUTO_INCREMENT de la tabla `empleados`
ALTER TABLE `empleados`
  MODIFY `idempleado` int(11) NOT NULL AUTO_INCREMENT;

-- AUTO_INCREMENT de la tabla `facturas`
ALTER TABLE `facturas`
  MODIFY `idfactura` int(11) NOT NULL AUTO_INCREMENT;
COMMIT;