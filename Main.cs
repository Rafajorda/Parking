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
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void btnTipos_Click(object sender, EventArgs e)
        {

        }

        private void brnVehiculos_Click(object sender, EventArgs e)
        {

        }

        // Abre el formulario para agregar un nuevo vehículo
        private void btnAgregar_Click(object sender, EventArgs e)
        {
            FormVehiculos formVehiculos = new FormVehiculos();
            formVehiculos.ShowDialog();
            refreshGrid();
        }

        private void dgvVehiculos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        // Carga inicial del formulario
        private void Main_Load(object sender, EventArgs e)
        {
            refreshGrid();
        }

        // Actualiza el DataGridView con los datos de la base de datos
        private void refreshGrid()
        {
            try
            {
                var db = new ParkingEntities();
                // Obtiene los vehículos con sus tipos usando proyección para mostrar solo el nombre del tipo
                var vehiculos = db.Vehiculos
                    .Include("TiposVehiculo")
                    .Select(vehiculo => new
                    {
                        vehiculo.Id,
                        vehiculo.Matricula,
                        vehiculo.Marca,
                        vehiculo.Modelo,
                        TipoVehiculo = vehiculo.TiposVehiculo.Nombre
                    })
                    .ToList();

                dgvVehiculos.DataSource = vehiculos;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los vehículos: " + ex.Message);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        // Busca vehículos por matrícula
        private void btnMatricula_Click(object sender, EventArgs e)
        {
            var db = new ParkingEntities();
            var vehiculos = db.Vehiculos
                .Include("TiposVehiculo").Where(v => v.Matricula.Contains(txtBuscar.Text))
                .Select(vehiculo => new
                {
                    vehiculo.Id,
                    vehiculo.Matricula,
                    vehiculo.Marca,
                    vehiculo.Modelo,
                    TipoVehiculo = vehiculo.TiposVehiculo.Nombre
                })
                .ToList();

            dgvVehiculos.DataSource = vehiculos;
        }

        // Restaura la vista completa de vehículos
        private void btnUndo_Click(object sender, EventArgs e)
        {
            refreshGrid();
        }

        // Abre el formulario para editar el vehículo seleccionado
        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (dgvVehiculos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Por favor, seleccione un vehículo para editar.");
                return;
            }
            else
            {
                // Obtiene el ID del vehículo seleccionado
                int selectedVehicleId = (int)dgvVehiculos.SelectedRows[0].Cells["Id"].Value;
                FormVehiculos formVehiculos = new FormVehiculos(selectedVehicleId);
                formVehiculos.ShowDialog();
                refreshGrid();
            }
        }

        // Elimina el vehículo seleccionado
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvVehiculos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Por favor, seleccione un vehículo para eliminar.");
                return;
            }
            else
            {
                DialogResult result = MessageBox.Show("¿Está seguro que desea eliminar este vehículo?", "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        int selectedVehicleId = (int)dgvVehiculos.SelectedRows[0].Cells["Id"].Value;
                        var db = new ParkingEntities();
                        var vehicleToDelete = db.Vehiculos.Find(selectedVehicleId);
                        if (vehicleToDelete != null)
                        {
                            db.Vehiculos.Remove(vehicleToDelete);
                            db.SaveChanges();
                            MessageBox.Show("Vehículo eliminado correctamente.");
                        }
                        refreshGrid();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al eliminar el vehículo: " + ex.Message);
                    }
                }
            }
        }
        private void btnTipos_Click_1(object sender, EventArgs e)
        {
            FormTiposV formTipos = new FormTiposV();
            formTipos.ShowDialog();
            refreshGrid();
        }
    }
}
