<?php
// 1. Reanudamos la sesión actual
session_start();

// 2. Destruimos todas las variables de la sesión
$_SESSION = array();

// 3. Destruimos la sesión en el servidor
session_destroy();

// 4. Redirigimos al usuario al login de forma limpia
header("Location: ingreso.html");
exit;
?>