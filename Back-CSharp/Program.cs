using System;
using MySql.Data.MySqlClient;

namespace Progra3Card.Administrativo
{
    class Program
    {
        private static string connectionString = "Server=localhost;Port=8889;Database=mi_banco_db;Uid=root;Pwd=root;";
        static void Main(string[] args)
        {
            bool salir = false;
            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("    SISTEMA ADMINISTRATIVO PROGRA3CARD   ");
                Console.WriteLine("========================================");
                Console.WriteLine("1. Emitir Nueva Tarjeta (Alta de Cliente)");
                Console.WriteLine("2. Listar Tarjetas");
                Console.WriteLine("3. Ver Detalle de una Tarjeta / Cliente");
                Console.WriteLine("4. Eliminar Tarjeta (Baja de Sistema)");
                Console.WriteLine("5. Emitir Nueva Liquidación Mensual");
                Console.WriteLine("6. Salir");
                Console.WriteLine("========================================");
                Console.Write("Seleccione una opción: ");

                switch (Console.ReadLine())
                {
                    case "1": MenuEmitirTarjeta(); break;
                    case "2": MenuListarTarjetas(); break;
                    case "3": MenuVerDetalleTarjeta(); break;
                    case "4": MenuEliminarTarjeta(); break;
                    case "5": MenuEmitirLiquidacion(); break;
                    case "6": salir = true; break;
                    default:
                        Console.WriteLine("\nOpción no válida. Presione una tecla para continuar...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        // =========================================================================
        // MÉTODOS DEL MENÚ (FLUJO DE CONSOLA)
        // =========================================================================

        static void MenuEmitirTarjeta()
        {
            Console.Clear();
            Console.WriteLine("==================================================");
            Console.WriteLine("   OPCIÓN 1: EMITIR NUEVA TARJETA / ALTA CLIENTE  ");
            Console.WriteLine("==================================================");

            // 1. Datos Personales del Cliente (Tabla: usuarios)
            Console.Write("Tipo de Documento (DNI/PASAPORTE): ");
            string tipoDoc = Console.ReadLine().ToUpper().Trim();
            if (tipoDoc != "DNI" && tipoDoc != "PASAPORTE")
            {
                Console.WriteLine("\nError: Tipo de documento inválido. Operación cancelada.");
                Console.ReadKey();
                return;
            }

            Console.Write("Número de Documento (Sin puntos): ");
            string documento = Console.ReadLine().Trim();

            Console.Write("Nombre: ");
            string nombre = Console.ReadLine().Trim();

            Console.Write("Apellido: ");
            string apellido = Console.ReadLine().Trim();

            Console.Write("Fecha de Nacimiento (AAAA-MM-DD): ");
            string fechaNac = Console.ReadLine().Trim();

            Console.Write("Email: ");
            string email = Console.ReadLine().Trim();

            // 2. Selección estricta del Banco Emisor (Tabla: tarjetas)
            Console.WriteLine("\nSeleccione el Banco Emisor:");
            Console.WriteLine("1. Banco Galicia");
            Console.WriteLine("2. Banco Nación");
            Console.WriteLine("3. Banco Santander");
            Console.Write("Opción: ");

            string bancoSeleccionado = "";
            switch (Console.ReadLine())
            {
                case "1": bancoSeleccionado = "Banco Galicia"; break;
                case "2": bancoSeleccionado = "Banco Nación"; break;
                case "3": bancoSeleccionado = "Banco Santander"; break;
                default:
                    Console.WriteLine("\nError: Selección de banco inválida. Operación cancelada.");
                    Console.ReadKey();
                    return;
            }

            // Datos automáticos del plástico
            Console.Write("\nIngrese los 16 dígitos del número de tarjeta física: ");
            string numeroTarjeta = Console.ReadLine().Trim();
            if (numeroTarjeta.Length != 16)
            {
                Console.WriteLine("\nError: El número de tarjeta debe tener exactamente 16 dígitos.");
                Console.ReadKey();
                return;
            }

            // 3. Proceso de inserción en Base de Datos usando una Transacción
            string queryUsuario = "INSERT INTO usuarios (documento, tipo_doc, nombre, apellido, fecha_nacimiento, email) VALUES (@doc, @tipo, @nom, @ape, @fNac, @email);";
            string queryTarjeta = "INSERT INTO tarjetas (numero_tarjeta, banco_emisor, estado, saldo, dni_titular) VALUES (@numT, @banco, 'Activa', 0.00, @doc);";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Iniciamos transacción por seguridad (si falla la tarjeta, no se crea el usuario incompleto)
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // Insertar Usuario
                            using (MySqlCommand cmdUser = new MySqlCommand(queryUsuario, conn, trans))
                            {
                                cmdUser.Parameters.AddWithValue("@doc", documento);
                                cmdUser.Parameters.AddWithValue("@tipo", tipoDoc);
                                cmdUser.Parameters.AddWithValue("@nom", nombre);
                                cmdUser.Parameters.AddWithValue("@ape", apellido);
                                cmdUser.Parameters.AddWithValue("@fNac", fechaNac);
                                cmdUser.Parameters.AddWithValue("@email", email);
                                cmdUser.ExecuteNonQuery();
                            }

                            // Insertar Tarjeta
                            using (MySqlCommand cmdTarj = new MySqlCommand(queryTarjeta, conn, trans))
                            {
                                cmdTarj.Parameters.AddWithValue("@numT", numeroTarjeta);
                                cmdTarj.Parameters.AddWithValue("@banco", bancoSeleccionado);
                                cmdTarj.Parameters.AddWithValue("@doc", documento);
                                cmdTarj.ExecuteNonQuery();
                            }

                            // Si ambas inserciones fueron exitosas, guardamos los cambios definitivamente
                            trans.Commit();
                            Console.WriteLine("\n¡Éxito! Cliente registrado y tarjeta emitida de forma correcta.");
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback(); // Si falla algo, deshace todo en MySQL
                            throw ex;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nError al procesar el alta en la base de datos: " + ex.Message);
                }
            }

            Console.WriteLine("\nPresione una tecla para regresar al menú...");
            Console.ReadKey();
        }

        static void MenuListarTarjetas()
        {
            Console.Clear();
            Console.WriteLine("=== LISTADO DE TARJETA ACTIVAS ===");
            ObtenerYMostrarTarjetas();
            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }

        static void MenuVerDetalleTarjeta()
        {
            Console.Clear();
            Console.WriteLine("=== VER DETALLE DE TARJETA Y CLIENTE ===");
            Console.Write("Ingrese el número de cuenta a consultar: ");
            if (int.TryParse(Console.ReadLine(), out int cuenta))
            {
                MostrarDetalleCompleto(cuenta);
            }
            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }

        static void MenuEliminarTarjeta()
        {
            Console.Clear();
            Console.WriteLine("=== ELIMINAR TARJETA DEL SISTEMA ===");
            Console.Write("Ingrese el número de cuenta a dar de baja: ");
            if (int.TryParse(Console.ReadLine(), out int numCuenta))
            {
                Console.Write($"¿Está seguro que desea eliminar la cuenta {numCuenta}? (S/N): ");
                if (Console.ReadLine().ToUpper() == "S")
                {
                    bool exito = DarDeBajaTarjeta(numCuenta);
                    if (exito)
                        Console.WriteLine("\nTarjeta eliminada correctamente del sistema.");
                    else
                        Console.WriteLine("\nError al intentar eliminar la tarjeta. Verifique el número de cuenta.");
                }
                else
                {
                    Console.WriteLine("\nOperación cancelada.");
                }
            }
            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }

        static void MenuEmitirLiquidacion()
        {
            Console.Clear();
            Console.WriteLine("==================================================");
            Console.WriteLine("        OPCIÓN 5: EMITIR NUEVA LIQUIDACIÓN        ");
            Console.WriteLine("==================================================");

            // 1. Solicitar el número de cuenta y verificar que exista
            Console.Write("Ingrese el Número de Cuenta de la tarjeta: ");
            if (!int.TryParse(Console.ReadLine(), out int numCuenta))
            {
                Console.WriteLine("\nError: Número de cuenta inválido.");
                Console.ReadKey();
                return;
            }

            // 2. Solicitar datos financieros del período
            Console.Write("Período (Formato AAAA-MM, ej. 2026-06): ");
            string periodo = Console.ReadLine().Trim();

            Console.Write("Fecha de Vencimiento (Formato AAAA-MM-DD): ");
            string fechaVenc = Console.ReadLine().Trim();

            Console.Write("Monto Total a Pagar (ej. 45200,50): ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal totalAPagar))
            {
                Console.WriteLine("\nError: Monto total inválido.");
                Console.ReadKey();
                return;
            }

            Console.Write("Monto de Pago Mínimo (ej. 8500,00): ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal pagoMinimo))
            {
                Console.WriteLine("\nError: Monto de pago mínimo inválido.");
                Console.ReadKey();
                return;
            }

            // 3. Inserción en la base de datos (Tabla: liquidaciones)
            string query = "INSERT INTO liquidaciones (periodo, fecha_vencimiento, total_a_pagar, pago_minimo, num_cuenta) " +
                           "VALUES (@periodo, @venc, @total, @minimo, @cuenta);";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@periodo", periodo);
                        cmd.Parameters.AddWithValue("@venc", fechaVenc);
                        cmd.Parameters.AddWithValue("@total", totalAPagar);
                        cmd.Parameters.AddWithValue("@minimo", pagoMinimo);
                        cmd.Parameters.AddWithValue("@cuenta", numCuenta);

                        int filasAfectadas = cmd.ExecuteNonQuery();

                        if (filasAfectadas > 0)
                        {
                            Console.WriteLine("\n¡Éxito! Nueva liquidación emitida correctamente.");
                            Console.WriteLine("El cliente ya puede visualizarla reflejada en el Portal Web.");
                        }
                        else
                        {
                            Console.WriteLine("\nError: No se pudo registrar la liquidación.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError al conectar e insertar la liquidación: " + ex.Message);
            }

            Console.WriteLine("\nPresione una tecla para regresar al menú...");
            Console.ReadKey();
        }


        // =========================================================================
        // MÉTODOS DE BASE DE DATOS QUE SE ENLAZAN CON LOS MENÚS
        // =========================================================================

        static void ObtenerYMostrarTarjetas()
        {
            Console.WriteLine("\n--------------------------------------------------------------------------------");
            Console.WriteLine("{0,-12} {1,-18} {2,-20} {3,-15} {4,-12}", "Nro Cuenta", "Nro Tarjeta", "Banco Emisor", "Titular (DNI)", "Estado");
            Console.WriteLine("--------------------------------------------------------------------------------");

            string query = "SELECT t.num_cuenta, t.numero_tarjeta, t.banco_emisor, t.dni_titular, t.estado " +
                           "FROM tarjetas t " +
                           "INNER JOIN usuarios u ON t.dni_titular = u.documento";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                Console.WriteLine("No se encontraron tarjetas registradas.");
                            }
                            else
                            {
                                while (reader.Read())
                                {
                                    Console.WriteLine("{0,-12} {1,-18} {2,-20} {3,-15} {4,-12}",
                                        reader["num_cuenta"],
                                        reader["numero_tarjeta"],
                                        reader["banco_emisor"],
                                        reader["dni_titular"],
                                        reader["estado"]);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error de conexión: " + ex.Message);
            }
            Console.WriteLine("--------------------------------------------------------------------------------");
        }

        static void MostrarDetalleCompleto(int cuenta)
        {
            // Consulta SQL que cruza todos los campos del usuario y su tarjeta correspondientes a la cuenta ingresada
            string query = "SELECT u.documento, u.tipo_doc, u.nombre, u.apellido, u.fecha_nacimiento, u.email, u.usuario, " +
                           "t.numero_tarjeta, t.banco_emisor, t.estado, t.saldo " +
                           "FROM tarjetas t " +
                           "INNER JOIN usuarios u ON t.dni_titular = u.documento " +
                           "WHERE t.num_cuenta = @cuenta";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@cuenta", cuenta);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Console.WriteLine("\n==================================================");
                                Console.WriteLine("          DETALLE DE CUENTA Y TITULAR            ");
                                Console.WriteLine("==================================================");
                                Console.WriteLine($"Nro. de Cuenta:    {cuenta}");
                                Console.WriteLine($"Banco Emisor:      {reader["banco_emisor"]}");
                                Console.WriteLine($"Número de Tarjeta: {reader["numero_tarjeta"]}");
                                Console.WriteLine($"Estado Comercial:  {reader["estado"]}");
                                Console.WriteLine($"Saldo Pendiente:   ${Convert.ToDecimal(reader["saldo"]):N2}");
                                Console.WriteLine("--------------------------------------------------");
                                Console.WriteLine("DATOS DEL TITULAR:");
                                Console.WriteLine($"Nombre Completo:   {reader["apellido"]}, {reader["nombre"]}");
                                Console.WriteLine($"Documento:         {reader["tipo_doc"]} {reader["documento"]}");

                                // Formateamos la fecha de nacimiento para que se vea prolija (DD/MM/AAAA)
                                DateTime fechaNac = Convert.ToDateTime(reader["fecha_nacimiento"]);
                                Console.WriteLine($"F. de Nacimiento:  {fechaNac.ToString("dd/MM/yyyy")}");
                                Console.WriteLine($"Email de Contacto: {reader["email"]}");
                                Console.WriteLine("--------------------------------------------------");

                                // Verificamos si ya completó el onboarding digital en la web
                                string estadoWeb = reader["usuario"] == DBNull.Value ? "PENDIENTE DE ACTIVACIÓN WEB" : $"ACTIVO (Usuario: {reader["usuario"]})";
                                Console.WriteLine($"Estado Portal Web: {estadoWeb}");
                                Console.WriteLine("==================================================");
                            }
                            else
                            {
                                Console.WriteLine($"\nError: No se encontró ninguna cuenta activa con el número {cuenta}.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError al consultar el detalle en la base de datos: " + ex.Message);
            }
        }

        static bool DarDeBajaTarjeta(int cuenta)
        {
            // Consulta para eliminar la tarjeta por su número de cuenta
            string query = "DELETE FROM tarjetas WHERE num_cuenta = @cuenta";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@cuenta", cuenta);

                        int filasAfectadas = cmd.ExecuteNonQuery();

                        // Si filasAfectadas es mayor a 0, significa que encontró la cuenta y la borró
                        return filasAfectadas > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError al intentar eliminar en la base de datos: " + ex.Message);
                return false;
            }
        }
    }
}