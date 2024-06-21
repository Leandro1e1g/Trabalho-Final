using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace SistemaGerenciamentoDoacoes
{
    public partial class MainForm : Form
    {
        private string connectionString = "Data Source=doacoes.db;Version=3;";
        private SQLiteConnection connection;

        public MainForm()
        {
            InitializeComponent();
            connection = new SQLiteConnection(connectionString);
            connection.Open();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            AtualizarTotalDoacoes();
        }

        private void btnRegistrarDoacao_Click(object sender, EventArgs e)
        {
            RegistrarDoacaoForm registrarForm = new RegistrarDoacaoForm(this);
            registrarForm.ShowDialog();
        }

        public void AtualizarTotalDoacoes()
        {
            string sql = "SELECT SUM(quantidade) FROM doacoes";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            object result = command.ExecuteScalar();
            if (result != DBNull.Value)
            {
                double total = Convert.ToDouble(result);
                lblTotalDoacoes.Text = $"Total de Doações: R$ {total:F2}";
            }
            else
            {
                lblTotalDoacoes.Text = "Total de Doações: R$ 0,00";
            }
        }

        private void btnConsultarDoacoes_Click(object sender, EventArgs e)
        {
            ConsultarDoacoesForm consultarForm = new ConsultarDoacoesForm();
            consultarForm.ShowDialog();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            connection.Close();
        }
    }

    public class Doacao
    {
        public int Id { get; set; }
        public string Tipo { get; set; }
        public double Quantidade { get; set; }
        public DateTime Data { get; set; }
    }

    public class DoacaoRepository
    {
        private SQLiteConnection connection;

        public DoacaoRepository(SQLiteConnection conn)
        {
            connection = conn;
        }

        public void AddDoacao(Doacao doacao)
        {
            string sql = "INSERT INTO doacoes (tipo, quantidade, data) VALUES (@tipo, @quantidade, @data)";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("@tipo", doacao.Tipo);
            command.Parameters.AddWithValue("@quantidade", doacao.Quantidade);
            command.Parameters.AddWithValue("@data", doacao.Data.ToString("yyyy-MM-dd"));
            command.ExecuteNonQuery();
        }

        public DataTable ConsultarDoacoes(string tipo, DateTime? dataInicio, DateTime? dataFim)
        {
            string sql = "SELECT * FROM doacoes WHERE 1 = 1";
            if (!string.IsNullOrEmpty(tipo))
            {
                sql += " AND tipo = @tipo";
            }
            if (dataInicio != null)
            {
                sql += " AND data >= @dataInicio";
            }
            if (dataFim != null)
            {
                sql += " AND data <= @dataFim";
            }

            SQLiteCommand command = new SQLiteCommand(sql, connection);
            if (!string.IsNullOrEmpty(tipo))
            {
                command.Parameters.AddWithValue("@tipo", tipo);
            }
            if (dataInicio != null)
            {
                command.Parameters.AddWithValue("@dataInicio", dataInicio.Value.ToString("yyyy-MM-dd"));
            }
            if (dataFim != null)
            {
                command.Parameters.AddWithValue("@dataFim", dataFim.Value.ToString("yyyy-MM-dd"));
            }

            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);
            return dataTable;
        }

        public double GetTotalDoacoes()
        {
            string sql = "SELECT SUM(quantidade) FROM doacoes";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            object result = command.ExecuteScalar();
            if (result != DBNull.Value)
            {
                return Convert.ToDouble(result);
            }
            else
            {
                return 0;
            }
        }
    }

    public partial class RegistrarDoacaoForm : Form
    {
        private MainForm mainForm;
        private SQLiteConnection connection;

        public RegistrarDoacaoForm(MainForm form)
        {
            InitializeComponent();
            mainForm = form;
            connection = form.Connection;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            string tipo = txtTipo.Text;
            double quantidade = Convert.ToDouble(txtQuantidade.Text);
            DateTime data = dateData.Value;

            Doacao doacao = new Doacao
            {
                Tipo = tipo,
                Quantidade = quantidade,
                Data = data
            };

            DoacaoRepository repository = new DoacaoRepository(connection);
            repository.AddDoacao(doacao);

            mainForm.AtualizarTotalDoacoes();
            MessageBox.Show("Doação registrada com sucesso!");

            LimparCampos();
        }

        private void LimparCampos()
        {
            txtTipo.Text = "";
            txtQuantidade.Text = "";
            dateData.Value = DateTime.Today;
        }
    }

    public partial class ConsultarDoacoesForm : Form
    {
        private SQLiteConnection connection;

        public ConsultarDoacoesForm()
        {
            InitializeComponent();
            connection = new SQLiteConnection("Data Source=doacoes.db;Version=3;");
            connection.Open();
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            string tipo = txtTipoConsulta.Text;
            DateTime? dataInicio = null;
            DateTime? dataFim = null;

            if (chkData.Checked)
            {
                dataInicio = dateInicio.Value;
                dataFim = dateFim.Value;
            }

            DoacaoRepository repository = new DoacaoRepository(connection);
            DataTable dataTable = repository.ConsultarDoacoes(tipo, dataInicio, dataFim);
            dgvConsultar.DataSource = dataTable;
        }

        private void ConsultarDoacoesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            connection.Close();
        }
    }
}