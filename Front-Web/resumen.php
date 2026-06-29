<?php
session_start();

if (!isset($_SESSION['usuario_logueado']) || $_SESSION['usuario_logueado'] !== true) {
    header("Location: ingreso.html");
    exit;
}

$host = "localhost";
$port = "8889"; 
$dbname = "mi_banco_db";
$user = "root";
$password = "root"; 

try {
    $conn = new PDO("mysql:host=$host;port=$port;dbname=$dbname;charset=utf8", $user, $password);
    $conn->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
} catch (PDOException $e) {
    die("Error de conexión: " . $e->getMessage());
}

$dni_usuario = $_SESSION['documento'];
$tarjeta = null;

try {
    // Ajustado exactamente a las columnas de tu Program.cs (numero_tarjeta, banco_emisor, saldo, estado)
    $sql_tarjeta = "SELECT numero_tarjeta, banco_emisor, saldo, estado FROM tarjetas WHERE dni_titular = :dni";
    $stmt_t = $conn->prepare($sql_tarjeta);
    $stmt_t->bindParam(':dni', $dni_usuario);
    $stmt_t->execute();
    $tarjeta = $stmt_t->fetch(PDO::FETCH_ASSOC);
} catch (PDOException $e) {
    // Manejo silencioso en caso de error
}
?>
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Mis Tarjetas - Panel Principal</title>
    <script src="https://cdn.tailwindcss.com"></script>
</head>
<body class="bg-gray-100 font-sans min-h-screen flex flex-col justify-between">

    <header class="bg-[#004691] text-white px-6 py-4 shadow-md flex justify-between items-center">
        <h1 class="text-xl font-semibold">Mis <span class="font-bold">Tarjetas</span></h1>
        <div class="flex items-center space-x-4">
            <span class="text-sm hidden sm:inline">Hola, <strong><?php echo htmlspecialchars($_SESSION['nombre'] . ' ' . $_SESSION['apellido']); ?></strong></span>
            <a href="cerrar.php" class="bg-red-600 hover:bg-red-700 text-white text-xs font-bold py-2 px-4 rounded-full transition">Cerrar Sesión</a>
        </div>
    </header>

    <main class="flex-grow p-6 max-w-5xl w-full mx-auto">
        <div class="bg-white rounded-lg shadow p-6 mb-6">
            <h2 class="text-2xl font-bold text-gray-800 mb-2">Bienvenido a tu Portal Web</h2>
            <p class="text-gray-600 text-sm">Monitoreo de productos Progra3card vinculados al documento <strong><?php echo htmlspecialchars($dni_usuario); ?></strong>.</p>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div class="md:col-span-1 bg-gradient-to-br from-blue-700 to-indigo-900 text-white rounded-2xl p-6 shadow-xl relative overflow-hidden flex flex-col justify-between h-48">
                <div>
                    <p class="text-xs uppercase opacity-75">Tarjeta de Crédito</p>
                    <h3 class="text-lg font-bold tracking-wider mt-1">
                        <?php echo $tarjeta ? htmlspecialchars($tarjeta['banco_emisor']) : 'Progra3card'; ?>
                    </h3>
                </div>
                <div class="my-4">
                    <p class="text-xl tracking-widest font-mono">
                        <?php 
                        if ($tarjeta && !empty($tarjeta['numero_tarjeta'])) {
                            echo "**** **** **** " . substr($tarjeta['numero_tarjeta'], -4);
                        } else {
                            echo "**** **** **** ****";
                        }
                        ?>
                    </p>
                </div>
                <div class="flex justify-between items-end">
                    <div>
                        <p class="text-[10px] uppercase opacity-50">Titular</p>
                        <p class="text-sm font-medium"><?php echo htmlspecialchars($_SESSION['nombre'] . ' ' . $_SESSION['apellido']); ?></p>
                    </div>
                    <div class="text-right">
                        <p class="text-[10px] uppercase opacity-50">Estado</p>
                        <span class="text-xs font-bold bg-green-500 text-white px-2 py-0.5 rounded">
                            <?php echo $tarjeta ? htmlspecialchars($tarjeta['estado']) : 'Activa'; ?>
                        </span>
                    </div>
                </div>
            </div>

            <div class="md:col-span-2 bg-white rounded-2xl p-6 shadow grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div class="border-b sm:border-b-0 sm:border-r border-gray-100 pb-4 sm:pb-0 sm:pr-4 flex flex-col justify-center">
                    <p class="text-xs font-semibold text-gray-400 uppercase">Saldo Pendiente Actual</p>
                    <p class="text-3xl font-bold text-gray-800 mt-1">
                        $<?php echo $tarjeta ? number_format($tarjeta['saldo'], 2, ',', '.') : '0,00'; ?>
                    </p>
                    <p class="text-xs text-gray-500 mt-1">Sincronizado en tiempo real desde C#</p>
                </div>
                <div class="flex flex-col justify-center pl-0 sm:pl-4">
                    <p class="text-xs font-semibold text-gray-400 uppercase">Seguridad Digital</p>
                    <p class="text-sm text-gray-600 mt-1">Recordá cerrar tu sesión de forma segura cuando termines de operar.</p>
                </div>
            </div>
        </div>
    </main>

    <footer class="bg-gray-50 text-[10px] text-gray-500 text-center p-4 border-t border-gray-200">
        Portal de Autogestión de Clientes - Progra3card.
    </footer>

</body>
</html>