using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Parking
{
    public partial class FormVehiculos : Form
    {
        // Variable para almacenar el ID del vehículo cuando se está editando
        private int selectedVehicleId = 0;

        // Constructor sin parámetros para agregar un nuevo vehículo
        public FormVehiculos()
        {
            InitializeComponent();
        }

        // Constructor con parámetros para editar un vehículo existente
        public FormVehiculos(int selectedVehicleId) : this()
        {
            this.selectedVehicleId = selectedVehicleId;
        }

        // Carga inicial del formulario
        private void FormVehiculos_Load(object sender, EventArgs e)
        {
            try
            {
                using (var db = new ParkingEntities())
                {
                    // Carga los tipos de vehículo en el ComboBox
                    var tipos = db.TiposVehiculo.ToList();
                    cmbTipo.DisplayMember = "Nombre";
                    cmbTipo.ValueMember = "Id";
                    cmbTipo.DataSource = tipos;

                    // Si estamos editando, cargar los datos del vehículo
                    if (selectedVehicleId > 0)
                    {
                        var vehiculo = db.Vehiculos.Find(selectedVehicleId);
                        if (vehiculo != null)
                        {
                            txtMatricula.Text = vehiculo.Matricula;
                            txtMarca.Text = vehiculo.Marca;
                            txtModelo.Text = vehiculo.Modelo;
                            cmbTipo.SelectedValue = vehiculo.TipoVehiculoId;
                        }

                        btnAgregar.Text = "Guardar Cambios";
                    }
                    else
                    {
                        cmbTipo.SelectedIndex = 0;
                        btnAgregar.Text = "Agregar";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los datos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Guarda un nuevo vehículo o actualiza uno existente
        private void btnAgregar_Click_1(object sender, EventArgs e)
        {
            // Validación: La matrícula no puede estar vacía
            if (string.IsNullOrWhiteSpace(txtMatricula.Text))
            {
                MessageBox.Show("La matrícula no puede estar vacía.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMatricula.Focus();
                return;
            }
            if (txtMatricula.Text.Length > 10)
            {
                MessageBox.Show("La matrícula no puede tener más de 10 caracteres.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMatricula.Focus();
                return;
            }
            try
            {
                using (var db = new ParkingEntities())
                {
                    var vehiculos = db.Vehiculos.Where(v => v.Matricula.Contains(txtMatricula.Text));
                  if (vehiculos.Any() && (selectedVehicleId == 0 || vehiculos.First().Id != selectedVehicleId))
                    {
                        MessageBox.Show("La matrícula ya existe. Por favor, ingrese una matrícula diferente.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtMatricula.Focus();
                        return;
                    }
                }
            }
            catch
            {
                MessageBox.Show ("error al validar la matrícula");
            }


            // comentados porque la base de datos dice que pueden ser null pero estan por si acaso

            //// Validación: La marca no puede estar vacía
            //if (string.IsNullOrWhiteSpace(txtMarca.Text))
            //{
            //    MessageBox.Show("La marca no puede estar vacía.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    txtMarca.Focus();
            //    return;
            //}

            //// Validación: El modelo no puede estar vacío
            //if (string.IsNullOrWhiteSpace(txtModelo.Text))
            //{
            //    MessageBox.Show("El modelo no puede estar vacío.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    txtModelo.Focus();
            //    return;
            //}


            // Validación: Debe seleccionar un tipo de vehículo
            if (cmbTipo.SelectedValue == null)
            {
                MessageBox.Show("Debe seleccionar un tipo de vehículo.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbTipo.Focus();
                return;
            }

            try
            {
                using (var db = new ParkingEntities())
                {
                    // Modo edición: actualiza el vehículo existente
                    if (selectedVehicleId > 0)
                    {
                        var vehiculo = db.Vehiculos.Find(selectedVehicleId);
                        if (vehiculo != null)
                        {
                            vehiculo.Matricula = txtMatricula.Text;
                            vehiculo.Marca = txtMarca.Text;
                            vehiculo.Modelo = txtModelo.Text;
                            vehiculo.TipoVehiculoId = (int)cmbTipo.SelectedValue;
                            db.SaveChanges();
                            MessageBox.Show("Vehículo actualizado correctamente.");
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("No se encontró el vehículo.");
                        }
                    }
                    // Modo agregar: crea un nuevo vehículo
                    else
                    {
                        var vehiculo = new Vehiculos
                        {
                            Matricula = txtMatricula.Text,
                            Marca = txtMarca.Text,
                            Modelo = txtModelo.Text,
                            TipoVehiculoId = (int)cmbTipo.SelectedValue
                        };
                        db.Vehiculos.Add(vehiculo);
                        db.SaveChanges();
                        MessageBox.Show("Vehículo agregado correctamente.");
                        this.Close();
                    }
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                // Manejo de errores de validación de Entity Framework
                string errorMessage = "Error de validación:\n";
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errorMessage += $"- {validationError.PropertyName}: {validationError.ErrorMessage}\n";
                    }
                }
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                // Manejo de errores de actualización de base de datos (claves duplicadas, restricciones, etc.)
                MessageBox.Show("Error al guardar en la base de datos. Puede que la matrícula ya exista o haya un problema con los datos.\n\nDetalle: " + ex.InnerException?.Message, 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Manejo de errores generales
                MessageBox.Show("Error inesperado: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
