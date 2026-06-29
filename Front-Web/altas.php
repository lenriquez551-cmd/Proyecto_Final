<?php
session_start();

$host = "localhost";
$port = "8889"; 
$dbname = "mi_banco_db";
$user = "root";
$password = "root"; 

try {
    $conn = new PDO("mysql:host=$host;port=$port;dbname=$dbname;charset=utf8", $user, $password);
    $conn->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
} catch (PDOException $e) {
    die("<div style='color:red; text-align:center; margin-top:20px;'><h3>Error de conexión: " . $e->getMessage() . "</h3></div>");
}

if ($_SERVER["REQUEST_METHOD"] == "POST") {
    
    $tipo_doc = trim($_POST['tipo_doc'] ?? '');
    $documento = trim($_POST['documento'] ?? '');
    $nombre = trim($_POST['nombre'] ?? '');
    $apellido = trim($_POST['apellido'] ?? '');
    $fecha_nacimiento = trim($_POST['fecha_nacimiento'] ?? '');
    $email = trim($_POST['email'] ?? '');
    $usuario = trim($_POST['usuario'] ?? '');
    $passwordA = trim($_POST['passwordA'] ?? '');
    $passwordB = trim($_POST['passwordB'] ?? '');

    if (empty($documento) || empty($usuario) || empty($passwordA) || empty($passwordB)) {
        echo "<script>alert('Por favor, completa los campos clave (Documento, Usuario y Contraseñas).'); window.history.back();</script>";
        exit;
    }

    if ($passwordA !== $passwordB) {
        echo "<script>alert('Error: Las contraseñas ingresadas no coinciden.'); window.history.back();</script>";
        exit;
    }

    try {
        // STEP 1: Verificar usando dni_titular igual que en C#
        $sql_check = "SELECT dni_titular FROM tarjetas WHERE dni_titular = :doc";
        $stmt_check = $conn->prepare($sql_check);
        $stmt_check->bindParam(':doc', $documento);
        $stmt_check->execute();

        if ($stmt_check->rowCount() == 0) {
            echo "<div style='text-align:center; margin-top:50px; font-family:Arial;'>";
            echo "<h2 style='color:#dc3545;'>No se pudo activar la cuenta</h2>";
            echo "<p>El documento <strong>$documento</strong> no figura con una tarjeta de crédito emitida en C#.</p>";
            echo "<br><a href='javascript:history.back()' style='padding:10px 20px; background:#004691; color:white; text-decoration:none; border-radius:4px;'>Volver a intentar</a>";
            echo "</div>";
            exit;
        }

        // STEP 2: Actualizar credenciales
        $sql_update = "UPDATE usuarios SET usuario = :usuario, password = :password WHERE documento = :doc";
        $stmt_update = $conn->prepare($sql_update);
        $stmt_update->bindParam(':usuario', $usuario);
        $stmt_update->bindParam(':password', $passwordA); 
        $stmt_update->bindParam(':doc', $documento);
        $stmt_update->execute();

        echo "<div style='text-align:center; margin-top:50px; font-family:Arial;'>";
        echo "<h2 style='color:#28a745; font-size:24px;'>¡Activación Web Exitosa!</h2>";
        echo "<p>El usuario web <strong>" . htmlspecialchars($usuario) . "</strong> ha sido creado correctamente.</p>";
        echo "<br><br><a href='ingreso.html' style='padding:10px 20px; background:#004691; color:white; text-decoration:none; border-radius:20px; font-weight:bold;'>Ir al Ingreso</a>";
        echo "</div>";

    } catch (PDOException $e) {
        echo "<script>alert('Error crítico: " . $e->getMessage() . "'); window.history.back();</script>";
    }
} else {
    header("Location: registro.html");
    exit;
}
?>