using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Parking
{
    public partial class FormTiposV : Form
    {
        // Variable para almacenar el ID del tipo de vehículo cuando se está editando
        private int selectedTipoId = 0;

        public FormTiposV()
        {
            InitializeComponent();
        }

        // Carga inicial del formulario
        private void FormTiposV_Load(object sender, EventArgs e)
        {
            refreshGrid();
        }

        // Guarda un nuevo tipo de vehículo o actualiza uno existente
        private void btnAgregar_Click(object sender, EventArgs e)
        {
            // Validación: El nombre no puede estar vacío
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("El nombre del tipo de vehículo no puede estar vacío.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            // Validación: La tarifa debe ser mayor a 0
            if (numTariff.Value <= 0)
            {
                MessageBox.Show("La tarifa por hora debe ser mayor a 0.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numTariff.Focus();
                return;
            }

            // Validación: El nombre no puede estar repetido
            try
            {
                using (var db = new ParkingEntities())
                {
                    var tipoExistente = db.TiposVehiculo.FirstOrDefault(v => v.Nombre == txtName.Text);
                    
                    // Si existe un tipo con ese nombre y no es el que estamos editando
                    if (tipoExistente != null && tipoExistente.Id != selectedTipoId)
                    {
                        MessageBox.Show("Ya existe un tipo de vehículo con ese nombre. Por favor, ingrese un nombre diferente.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtName.Focus();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al validar el nombre: " + ex.Message);
                return;
            }

            try
            {
                using (var db = new ParkingEntities())
                {
                    // Modo edición: actualiza el tipo de vehículo existente
                    if (selectedTipoId > 0)
                    {
                        var tipo = db.TiposVehiculo.Find(selectedTipoId);
                        if (tipo != null)
                        {
                            tipo.Nombre = txtName.Text;
                            tipo.TarifaHora = numTariff.Value;
                            db.SaveChanges();
                            MessageBox.Show("Tipo de vehículo actualizado correctamente");
                        }
                    }
                    // Modo agregar: crea un nuevo tipo de vehículo
                    else
                    {
                        var tipo = new TiposVehiculo
                        {
                            Nombre = txtName.Text,
                            TarifaHora = numTariff.Value,
                        };
                        db.TiposVehiculo.Add(tipo);
                        db.SaveChanges();
                        MessageBox.Show("Tipo de vehículo agregado correctamente");
                    }

                    limpiarCampos();
                    refreshGrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message);
            }
        }

        // Actualiza el DataGridView con los datos de la base de datos
        private void refreshGrid()
        {
            try
            {
                using (var db = new ParkingEntities())
                {
                    // Proyección para mostrar solo los campos necesarios
                    var tipos = db.TiposVehiculo
                        .Select(tipo => new
                        {
                            tipo.Id,
                            tipo.Nombre,
                            tipo.TarifaHora
                        })
                        .ToList();

                    dgvTipos.DataSource = tipos;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los vehículos: " + ex.Message);
            }
        }

        // Elimina el tipo de vehículo seleccionado
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvTipos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Por favor, seleccione un tipo de vehículo para eliminar.");
                return;
            }
            else
            {
                try
                {
                    int selectedVehicleId = (int)dgvTipos.SelectedRows[0].Cells["Id"].Value;
                    
                    using (var db = new ParkingEntities())
                    {
                        // Verifica si existen vehículos con este tipo
                        var vehiculosConTipo = db.Vehiculos.Where(v => v.TipoVehiculoId == selectedVehicleId).ToList();
                        
                        if (vehiculosConTipo.Any())
                        {
                            MessageBox.Show($"No se puede eliminar este tipo de vehículo porque hay {vehiculosConTipo.Count} vehículo(s) asociado(s) a él.", "No se puede eliminar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    
                    DialogResult result = MessageBox.Show("¿Está seguro que desea eliminar este tipo de vehículo?", "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    
                    if (result == DialogResult.Yes)
                    {
                        using (var db = new ParkingEntities())
                        {
                            var typeToDelete = db.TiposVehiculo.Find(selectedVehicleId);
                            if (typeToDelete != null)
                            {
                                db.TiposVehiculo.Remove(typeToDelete);
                                db.SaveChanges();
                                MessageBox.Show("Tipo de vehículo eliminado correctamente.");
                            }
                            refreshGrid();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al eliminar el tipo de vehiculo: " + ex.Message);
                }
            }
        }

        // Carga los datos del tipo de vehículo seleccionado para editar o cancela la edición
        private void btnEditar_Click(object sender, EventArgs e)
        {
            // Si ya está en modo edición, cancela y limpia los campos
            if (selectedTipoId > 0)
            {
                limpiarCampos();
                return;
            }

            if (dgvTipos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Por favor, seleccione un tipo de vehículo para editar.");
                return;
            }

            // Obtiene el ID del tipo de vehículo seleccionado
            selectedTipoId = Convert.ToInt32(dgvTipos.SelectedRows[0].Cells["Id"].Value);

            try
            {
                using (var db = new ParkingEntities())
                {
                    var tipo = db.TiposVehiculo.Find(selectedTipoId);
                    if (tipo != null)
                    {
                        // Carga los datos en los campos de texto
                        txtName.Text = tipo.Nombre;
                        numTariff.Value = tipo.TarifaHora;
                        // Cambia la interfaz al modo edición
                        btnAgregar.Text = "Guardar Cambios";
                        btnEditar.Text = "Cancelar";
                        btnEditar.BackColor = Color.Red;

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los datos: " + ex.Message);
            }
        }

        // Limpia los campos y resetea el formulario al modo agregar
        private void limpiarCampos()
        {
            txtName.Clear();
            numTariff.Value = 0;
            selectedTipoId = 0;
            btnAgregar.Text = "Agregar";
            btnEditar.Text = "Editar";
            btnEditar.BackColor = Color.LightSeaGreen;
            dgvTipos.ClearSelection();
        }
    }
}
