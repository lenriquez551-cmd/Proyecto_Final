<?php
// 1. Iniciar la sesión antes que cualquier otra cosa
session_start();

// 2. Configuración de la conexión (MAMP - Puerto estándar)
$host = "localhost";
$port = "8889"; // Puerto MySQL de MAMP
$dbname = "mi_banco_db";
$user = "root";
$password = "root"; 

try {
    $conn = new PDO("mysql:host=$host;port=$port;dbname=$dbname;charset=utf8", $user, $password);
    $conn->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
} catch (PDOException $e) {
    die("<div style='color:red; text-align:center; margin-top:20px;'><h3>Error de conexión: " . $e->getMessage() . "</h3></div>");
}

// 3. Procesar el formulario
if ($_SERVER["REQUEST_METHOD"] == "POST") {
    
    $documento = trim($_POST['documento'] ?? '');
    $usuario = trim($_POST['usuario'] ?? '');
    $pass = trim($_POST['password'] ?? '');

    if (empty($documento) || empty($usuario) || empty($pass)) {
        echo "<script>alert('Por favor, completa todos los campos.'); window.history.back();</script>";
        exit;
    }

    try {
        // Buscamos coincidencia exacta en la tabla usuarios
        $sql = "SELECT documento, nombre, apellido, email FROM usuarios 
                WHERE documento = :doc AND usuario = :user AND password = :pass";
        
        $stmt = $conn->prepare($sql);
        $stmt->bindParam(':doc', $documento);
        $stmt->bindParam(':user', $usuario);
        $stmt->bindParam(':pass', $pass);
        $stmt->execute();

        // Si el usuario existe y la clave es correcta
        if ($stmt->rowCount() > 0) {
            $datosUsuario = $stmt->fetch(PDO::FETCH_ASSOC);
            
            // Guardamos las variables de sesión
            $_SESSION['usuario_logueado'] = true;
            $_SESSION['documento'] = $datosUsuario['documento'];
            $_SESSION['nombre'] = $datosUsuario['nombre'];
            $_SESSION['apellido'] = $datosUsuario['apellido'];
            $_SESSION['email'] = $datosUsuario['email'];

            // Redirigimos a la pantalla de resúmenes
            header("Location: resumen.php");
            exit;
        } else {
            // Datos incorrectos
            echo "<script>
                alert('Error: Documento, usuario o contraseña incorrectos.');
                window.history.back();
            </script>";
            exit;
        }

    } catch (PDOException $e) {
        die("Error en la consulta: " . $e->getMessage());
    }
} else {
    header("Location: ingreso.html");
    exit;
}
?>