﻿using Sigran.Classes;
using Sigran.Forms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using MenuItem = System.Windows.Forms.MenuItem;
using Series = System.Windows.Forms.DataVisualization.Charting.Series;
using System.Runtime.InteropServices;

namespace Sigran
{
    public partial class MainForm : Form
    {
        public static string AppName = "Sigran";
        public static string AppDescription = "Sistema de Informação Granulométrica";
        public static string AppVersion = "1.2";
        public string DatabaseFile = null;
        public string DefaultPath = null;
        public string SettingsFile = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData), AppName + " " + AppVersion + " - Settings.ini");
        public bool IsSave = false;
        public bool dev = false;
        public bool reload = false;
        public bool createSetupFile = false;

        public bool fileMenuOpened = false;

        internal static Functions f = new Functions();

        internal static SampleTools sampleTools = new SampleTools();

        public TreeNodeCollection treeNode { get; }

        public MainForm()
        {
            InitializeComponent();
            this.Text = AppName + " - " + AppDescription;
            dataGridView.Visible = false;
            dataGridView.Dock = DockStyle.Fill;
            defaultView.Visible = true;
            defaultView.Dock = DockStyle.Fill;

            IniFile ini = new IniFile(this.SettingsFile);
            reload = (ini.Read("RELOAD") == "1") ? true : false;

            if (dev == true || reload == true)
            {
                DatabaseFile = "Teste.db";

                if (reload == true)
                    DatabaseFile = ini.Read("LASTOPENED");

                DatabaseConnect database = new DatabaseConnect(DatabaseFile);
                database.CreateDatabase();

                dataGridView.Visible = true;
                defaultView.Visible = false;

                this.Text = AppName + " (Teste.db)";

                if (reload == true)
                    this.Text = AppName + " (" + ini.Read("LASTOPENED") + ")";

                salvarComoToolStripMenuItem.Enabled = true;
                fecharToolStripMenuItem.Enabled = true;
                importarToolStripMenuItem.Enabled = true;
                exportarToolStripMenuItem.Enabled = true;
                amostraToolStripMenuItem.Enabled = true;
                processarToolStripMenuItem.Enabled = true;

                button2.Enabled = button3.Enabled = button5.Enabled = button6.Enabled = button7.Enabled = button8.Enabled = button9.Enabled = button10.Enabled = button11.Enabled = button12.Enabled = button13.Enabled = button14.Enabled = button15.Enabled = button16.Enabled = true;
            }

            updateDataGrid(null, null, null, true);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //Criar Arquivo de script de setup
            if (createSetupFile)
            {
                CreateSetupScriptFile.Execute();
            }

            //HistogramaToolStripMenuItem_Click(sender, e);
            System.Windows.Forms.ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
            ToolTip1.SetToolTip(this.button1, this.button1.Tag.ToString());
            ToolTip1.SetToolTip(this.button2, this.button2.Tag.ToString());
            ToolTip1.SetToolTip(this.button3, this.button3.Tag.ToString());
            ToolTip1.SetToolTip(this.button5, this.button5.Tag.ToString());
            ToolTip1.SetToolTip(this.button6, this.button6.Tag.ToString());
            ToolTip1.SetToolTip(this.button7, this.button7.Tag.ToString());
            ToolTip1.SetToolTip(this.button15, this.button15.Tag.ToString());
            ToolTip1.SetToolTip(this.button16, this.button16.Tag.ToString());
            ToolTip1.SetToolTip(this.button8, this.button8.Tag.ToString());
            ToolTip1.SetToolTip(this.button9, this.button9.Tag.ToString());
            ToolTip1.SetToolTip(this.button10, this.button10.Tag.ToString());
            ToolTip1.SetToolTip(this.button11, this.button11.Tag.ToString());
            ToolTip1.SetToolTip(this.button12, this.button12.Tag.ToString());
            ToolTip1.SetToolTip(this.button13, this.button13.Tag.ToString());
            ToolTip1.SetToolTip(this.button14, this.button14.Tag.ToString());

            if (dev == false && reload == false)
                button1.Enabled = button2.Enabled = button3.Enabled = button5.Enabled = button6.Enabled = button7.Enabled = button8.Enabled = button9.Enabled = button10.Enabled = button11.Enabled = button12.Enabled = button13.Enabled = button14.Enabled = button15.Enabled = button16.Enabled = false;

        }

        private void NovoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Criando novo projeto...");
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "Arquivo de banco de dados (*.db)|*.db";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.Title = "Criar novo arquivo do banco de dados";
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                DatabaseFile = saveFileDialog1.FileName;

                DatabaseConnect database = new DatabaseConnect(DatabaseFile);
                database.CreateDatabase();

                dataGridView.Visible = true;
                defaultView.Visible = false;

                this.Text = AppName + " (" + saveFileDialog1.FileName + ")";
                Console.WriteLine("Projeto " + saveFileDialog1.FileName + " criado com sucesso...");

                salvarComoToolStripMenuItem.Enabled = true;
                fecharToolStripMenuItem.Enabled = true;
                importarToolStripMenuItem.Enabled = true;
                exportarToolStripMenuItem.Enabled = true;
                amostraToolStripMenuItem.Enabled = true;
                processarToolStripMenuItem.Enabled = true;

                button2.Enabled = button3.Enabled = button5.Enabled = button6.Enabled = button7.Enabled = button8.Enabled = button9.Enabled = button10.Enabled = button11.Enabled = button12.Enabled = button13.Enabled = button14.Enabled = button15.Enabled = button16.Enabled = true;
            }

            if (DatabaseFile != null)
            {
                updateDataGrid(null, null, null, true);
            }
        }

        private void AbrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Abrindo projeto...");
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Arquivo de banco de dados (*.db)|*.db";
            openFileDialog1.Title = "Abrir arquivo do banco de dados";
            openFileDialog1.FileName = DefaultPath;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                dataGridView.Visible = true;
                defaultView.Visible = false;

                DatabaseFile = openFileDialog1.FileName;
                this.Text = AppName + " (" + openFileDialog1.FileName + ")";
                Console.WriteLine("Projeto " + openFileDialog1.FileName + " carregado com sucesso...");

                salvarComoToolStripMenuItem.Enabled = true;
                fecharToolStripMenuItem.Enabled = true;
                importarToolStripMenuItem.Enabled = true;
                exportarToolStripMenuItem.Enabled = true;
                amostraToolStripMenuItem.Enabled = true;
                processarToolStripMenuItem.Enabled = true;

                button2.Enabled = button3.Enabled = button5.Enabled = button6.Enabled = button7.Enabled = button8.Enabled = button9.Enabled = button10.Enabled = button11.Enabled = button12.Enabled = button13.Enabled = button14.Enabled = button15.Enabled = button16.Enabled = true;

                IniFile ini = new IniFile(this.SettingsFile);
                ini.Write("LASTOPENED", openFileDialog1.FileName);
            }

            if (DatabaseFile != null)
            {
                updateDataGrid(null, null, null, true);
            }
        }

        public void updateDataGrid(string amostra, string categoria, string data, bool updateTree)
        {
            if(DatabaseFile != null)
            {
                dataGridView.Rows.Clear();

                DatabaseConnect database = new DatabaseConnect(DatabaseFile);
                string sql = "SELECT * FROM Samples";
                SQLiteConnection conn = new SQLiteConnection(database.Connection);
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                SQLiteDataReader dr = cmd.ExecuteReader();
                List<string> listaCategorias = new List<string>();
                List<string> listaAmostras = new List<string>();
                List<string> listaDatas = new List<string>();
                while (dr.Read())
                {
                    if (categoria == null && amostra == null && data == null)
                    {
                        this.dataGridView.Rows.Add(
                            Convert.ToInt32(dr["Id"]),
                            dr["Name"].ToString().Replace(".", ","),
                            dr["Category"].ToString().Replace(".", ","),
                            dr["Description"].ToString().Replace(".", ","),
                            dr["Date"].ToString().Replace(".", ","),
                            dr["Carbonates"].ToString().Replace(".", ","),
                            dr["Latitude"].ToString().Replace(".", ","),
                            dr["Longitude"].ToString().Replace(".", ","),
                            dr["Weight0"].ToString().Replace(".", ","),
                            dr["Weight1"].ToString().Replace(".", ","),
                            dr["Weight2"].ToString().Replace(".", ","),
                            dr["Weight3"].ToString().Replace(".", ","),
                            dr["Weight4"].ToString().Replace(".", ","),
                            dr["Weight5"].ToString().Replace(".", ","),
                            dr["Weight6"].ToString().Replace(".", ","),
                            dr["Weight7"].ToString().Replace(".", ","),
                            dr["Weight8"].ToString().Replace(".", ","),
                            dr["Weight9"].ToString().Replace(".", ","),
                            dr["Weight10"].ToString().Replace(".", ","),
                            dr["Weight11"].ToString().Replace(".", ","),
                            dr["Weight12"].ToString().Replace(".", ","),
                            dr["Weight13"].ToString().Replace(".", ","),
                            dr["Weight14"].ToString().Replace(".", ","),
                            dr["Weight15"].ToString().Replace(".", ","),
                            dr["Weight16"].ToString().Replace(".", ","),
                            dr["Weight17"].ToString().Replace(".", ","),
                            dr["Weight18"].ToString().Replace(".", ","),
                            dr["Weight19"].ToString().Replace(".", ","),
                            dr["Weight20"].ToString().Replace(".", ","),
                            dr["Weight21"].ToString().Replace(".", ","),
                            dr["Weight22"].ToString().Replace(".", ","),
                            dr["Weight23"].ToString().Replace(".", ","),
                            dr["Weight24"].ToString().Replace(".", ","),
                            dr["Weight25"].ToString()
                        );
                    }
                    else
                    {
                        if (categoria == dr["Category"].ToString() && categoria != null)
                        {
                            this.dataGridView.Rows.Add(
                                Convert.ToInt32(dr["Id"]),
                                dr["Name"].ToString().Replace(".", ","),
                                dr["Category"].ToString().Replace(".", ","),
                                dr["Description"].ToString().Replace(".", ","),
                                dr["Date"].ToString().Replace(".", ","),
                                dr["Carbonates"].ToString().Replace(".", ","),
                                dr["Latitude"].ToString().Replace(".", ","),
                                dr["Longitude"].ToString().Replace(".", ","),
                                dr["Weight0"].ToString().Replace(".", ","),
                                dr["Weight1"].ToString().Replace(".", ","),
                                dr["Weight2"].ToString().Replace(".", ","),
                                dr["Weight3"].ToString().Replace(".", ","),
                                dr["Weight4"].ToString().Replace(".", ","),
                                dr["Weight5"].ToString().Replace(".", ","),
                                dr["Weight6"].ToString().Replace(".", ","),
                                dr["Weight7"].ToString().Replace(".", ","),
                                dr["Weight8"].ToString().Replace(".", ","),
                                dr["Weight9"].ToString().Replace(".", ","),
                                dr["Weight10"].ToString().Replace(".", ","),
                                dr["Weight11"].ToString().Replace(".", ","),
                                dr["Weight12"].ToString().Replace(".", ","),
                                dr["Weight13"].ToString().Replace(".", ","),
                                dr["Weight14"].ToString().Replace(".", ","),
                                dr["Weight15"].ToString().Replace(".", ","),
                                dr["Weight16"].ToString().Replace(".", ","),
                                dr["Weight17"].ToString().Replace(".", ","),
                                dr["Weight18"].ToString().Replace(".", ","),
                                dr["Weight19"].ToString().Replace(".", ","),
                                dr["Weight20"].ToString().Replace(".", ","),
                                dr["Weight21"].ToString().Replace(".", ","),
                                dr["Weight22"].ToString().Replace(".", ","),
                                dr["Weight23"].ToString().Replace(".", ","),
                                dr["Weight24"].ToString().Replace(".", ","),
                                dr["Weight25"].ToString()
                            );
                        }

                        if (amostra == dr["Name"].ToString() && amostra != null)
                        {
                            this.dataGridView.Rows.Add(
                                Convert.ToInt32(dr["Id"]),
                                dr["Name"].ToString().Replace(".", ","),
                                dr["Category"].ToString().Replace(".", ","),
                                dr["Description"].ToString().Replace(".", ","),
                                dr["Date"].ToString().Replace(".", ","),
                                dr["Carbonates"].ToString().Replace(".", ","),
                                dr["Latitude"].ToString().Replace(".", ","),
                                dr["Longitude"].ToString().Replace(".", ","),
                                dr["Weight0"].ToString().Replace(".", ","),
                                dr["Weight1"].ToString().Replace(".", ","),
                                dr["Weight2"].ToString().Replace(".", ","),
                                dr["Weight3"].ToString().Replace(".", ","),
                                dr["Weight4"].ToString().Replace(".", ","),
                                dr["Weight5"].ToString().Replace(".", ","),
                                dr["Weight6"].ToString().Replace(".", ","),
                                dr["Weight7"].ToString().Replace(".", ","),
                                dr["Weight8"].ToString().Replace(".", ","),
                                dr["Weight9"].ToString().Replace(".", ","),
                                dr["Weight10"].ToString().Replace(".", ","),
                                dr["Weight11"].ToString().Replace(".", ","),
                                dr["Weight12"].ToString().Replace(".", ","),
                                dr["Weight13"].ToString().Replace(".", ","),
                                dr["Weight14"].ToString().Replace(".", ","),
                                dr["Weight15"].ToString().Replace(".", ","),
                                dr["Weight16"].ToString().Replace(".", ","),
                                dr["Weight17"].ToString().Replace(".", ","),
                                dr["Weight18"].ToString().Replace(".", ","),
                                dr["Weight19"].ToString().Replace(".", ","),
                                dr["Weight20"].ToString().Replace(".", ","),
                                dr["Weight21"].ToString().Replace(".", ","),
                                dr["Weight22"].ToString().Replace(".", ","),
                                dr["Weight23"].ToString().Replace(".", ","),
                                dr["Weight24"].ToString().Replace(".", ","),
                                dr["Weight25"].ToString()
                            );
                        }

                        if (data == dr["Date"].ToString() && data != null)
                        {
                            this.dataGridView.Rows.Add(
                                Convert.ToInt32(dr["Id"]),
                                dr["Name"].ToString().Replace(".", ","),
                                dr["Category"].ToString().Replace(".", ","),
                                dr["Description"].ToString().Replace(".", ","),
                                dr["Date"].ToString().Replace(".", ","),
                                dr["Carbonates"].ToString().Replace(".", ","),
                                dr["Latitude"].ToString().Replace(".", ","),
                                dr["Longitude"].ToString().Replace(".", ","),
                                dr["Weight0"].ToString().Replace(".", ","),
                                dr["Weight1"].ToString().Replace(".", ","),
                                dr["Weight2"].ToString().Replace(".", ","),
                                dr["Weight3"].ToString().Replace(".", ","),
                                dr["Weight4"].ToString().Replace(".", ","),
                                dr["Weight5"].ToString().Replace(".", ","),
                                dr["Weight6"].ToString().Replace(".", ","),
                                dr["Weight7"].ToString().Replace(".", ","),
                                dr["Weight8"].ToString().Replace(".", ","),
                                dr["Weight9"].ToString().Replace(".", ","),
                                dr["Weight10"].ToString().Replace(".", ","),
                                dr["Weight11"].ToString().Replace(".", ","),
                                dr["Weight12"].ToString().Replace(".", ","),
                                dr["Weight13"].ToString().Replace(".", ","),
                                dr["Weight14"].ToString().Replace(".", ","),
                                dr["Weight15"].ToString().Replace(".", ","),
                                dr["Weight16"].ToString().Replace(".", ","),
                                dr["Weight17"].ToString().Replace(".", ","),
                                dr["Weight18"].ToString().Replace(".", ","),
                                dr["Weight19"].ToString().Replace(".", ","),
                                dr["Weight20"].ToString().Replace(".", ","),
                                dr["Weight21"].ToString().Replace(".", ","),
                                dr["Weight22"].ToString().Replace(".", ","),
                                dr["Weight23"].ToString().Replace(".", ","),
                                dr["Weight24"].ToString().Replace(".", ","),
                                dr["Weight25"].ToString()
                            );
                        }
                    }

                    if (!listaCategorias.Contains(dr["Category"].ToString()) && dr["Category"].ToString() != "")
                        listaCategorias.Add(dr["Category"].ToString());

                    if (!listaAmostras.Contains(dr["Name"].ToString()) && dr["Name"].ToString() != "")
                        listaAmostras.Add(dr["Name"].ToString());

                    if (!listaDatas.Contains(dr["Date"].ToString()) && dr["Date"].ToString() != "")
                        listaDatas.Add(dr["Date"].ToString());

                }

                conn.Close();

                listaCategorias = listaCategorias.OrderBy(q => q).ToList();
                listaAmostras = listaAmostras.OrderBy(q => q).ToList();
                listaDatas = listaDatas.OrderBy(q => q).ToList();

                if (updateTree == true)
                {
                    //Tree

                    //Amostras
                    TreeNodeCollection treeNode = treeView.Nodes;
                    treeNode.Clear();
                    treeNode.Add("Amostras", "Amostras");
                    treeNode["Amostras"].Nodes.Add(new TreeNode("Todas"));
                    treeView.ExpandAll();
                    foreach (string i in listaAmostras)
                    {
                        treeNode["Amostras"].Nodes.Add(new TreeNode(i));
                    }

                    //Categorias
                    treeNode.Add("Categorias", "Categorias");
                    treeNode["Categorias"].Nodes.Add(new TreeNode("Todas"));
                    foreach (string i in listaCategorias)
                    {
                        treeNode["Categorias"].Nodes.Add(new TreeNode(i));
                    }

                    //Datas
                    treeNode.Add("Datas", "Datas");
                    treeNode["Datas"].Nodes.Add(new TreeNode("Todas"));
                    foreach (string i in listaDatas)
                    {
                        treeNode["Datas"].Nodes.Add(new TreeNode(i));
                    }
                }
            }
            

        }

        public List<string> getAllCategories()
        {
            List<string> listaCategorias = new List<string>();

            DatabaseConnect database = new DatabaseConnect(DatabaseFile);
            string sql = "SELECT * FROM Samples";
            SQLiteConnection conn = new SQLiteConnection(database.Connection);
            if (conn.State == ConnectionState.Closed)
                conn.Open();
            SQLiteCommand cmd = new SQLiteCommand(sql, conn);
            SQLiteDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                if (!listaCategorias.Contains(dr["Category"].ToString()))
                {
                    listaCategorias.Add(dr["Category"].ToString());
                }
            }

            conn.Close();

            listaCategorias = listaCategorias.OrderBy(q => q).ToList();

            return listaCategorias;
        }
        
        private void SalvarComoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Salvando projeto como...");
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "Arquivo de banco de dados (*.db)|*.db";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.Title = "Salvar banco de dados como...";
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string DatabaseFileOld = DatabaseFile;
                DatabaseFile = saveFileDialog1.FileName;

                System.IO.File.Copy(DatabaseFileOld, DatabaseFile, true);

                DatabaseConnect database = new DatabaseConnect(DatabaseFile);
                database.CreateDatabase();

                dataGridView.Visible = true;
                defaultView.Visible = false;

                this.Text = AppName + " (" + saveFileDialog1.FileName + ")";
                Console.WriteLine("Projeto " + saveFileDialog1.FileName + " criado com sucesso...");

                salvarComoToolStripMenuItem.Enabled = true;
                fecharToolStripMenuItem.Enabled = true;
                importarToolStripMenuItem.Enabled = true;
                exportarToolStripMenuItem.Enabled = true;
                amostraToolStripMenuItem.Enabled = true;
                processarToolStripMenuItem.Enabled = true;

                //Atualiza a área de trabalho
                updateDataGrid(null, null, null, true);
            }
        }

        private void FecharToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView.Rows.Clear();
            treeView.Nodes.Clear();

            DatabaseFile = null;
            DefaultPath = null;
            IsSave = false;
            this.Text = AppName + " - " + AppDescription;
            dataGridView.Visible = false;
            dataGridView.Dock = DockStyle.Fill;
            defaultView.Visible = true;
            defaultView.Dock = DockStyle.Fill;

            salvarComoToolStripMenuItem.Enabled = false;
            fecharToolStripMenuItem.Enabled = false;
            importarToolStripMenuItem.Enabled = false;
            exportarToolStripMenuItem.Enabled = false;
            amostraToolStripMenuItem.Enabled = false;
            processarToolStripMenuItem.Enabled = false;

            button1.Enabled = button2.Enabled = button3.Enabled = button5.Enabled = button6.Enabled = button7.Enabled = button8.Enabled = button9.Enabled = button10.Enabled = button11.Enabled = button12.Enabled = button13.Enabled = button14.Enabled = button15.Enabled = button16.Enabled = false;
        }

        private void ImportarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Importar dados...");
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Arquivo de banco de dados do "+AppName+" (*.db)|*.db|Arquivo de banco de dados do SAG (*.mdb)|*.mdb";
            //openFileDialog1.Filter = "Arquivo de banco de dados do " + AppName + " (*.db)|*.db|Arquivo de banco de dados do SAG (*.mdb)|*.mdb|Planília do Excel do SysGran (*.xls)|*.xls";
            openFileDialog1.Title = "Importar dados externos";
            openFileDialog1.FileName = DefaultPath;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string FileName = openFileDialog1.FileName;

                Imports imports = new Imports();

                bool resultado = false;

                //IMPORT FROM .DB
                if (FileName.Contains(".db") || FileName.Contains(".DB"))
                {
                    List<Sample> listaDados = imports.fromDbFile(@FileName);

                    foreach (var dados in listaDados)
                    {
                        //Insere dados no banco de dados
                        DatabaseConnect database = new DatabaseConnect(DatabaseFile);
                        resultado = database.Insert(dados, "Samples", false);

                        updateDataGrid(null, null, null, true);
                    }
                }

                //IMPORT FROM .MDB
                if (FileName.Contains(".mdb") || FileName.Contains(".MDB"))
                {
                    List<Sample> listaDados = imports.fromMdbFile(@FileName);

                    foreach (var dados in listaDados)
                    {
                        //Insere dados no banco de dados
                        DatabaseConnect database = new DatabaseConnect(DatabaseFile);
                        resultado = database.Insert(dados, "Samples", false);

                        updateDataGrid(null, null, null, true);
                    }

                }

                //IMPORT FROM .XLS
                /*if (FileName.Contains(".xls") || FileName.Contains(".XLS"))
                {
                    List<Sample> listaDados = imports.fromXlsFile(@FileName);

                    foreach (var dados in listaDados)
                    {
                        //Insere dados no banco de dados
                        DatabaseConnect database = new DatabaseConnect(DatabaseFile);
                        resultado = database.Insert(dados, "Samples", false);

                        updateDataGrid(null, true);
                    }
                }*/


                if (resultado)
                {
                    MessageBox.Show("Dados importados com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Erro ao importar dados.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }

            if (DatabaseFile != null)
            {
                updateDataGrid(null, null, null, true);
            }
        }

        private void ExportarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            f.exportDatagridToFile(dataGridView, true);
        }

        private void SairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void InserirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Cria o form
            SampleForm form = new SampleForm();
            form.Owner = this;
            form.MdiParent = MdiParent;
            form.DatabaseFile = DatabaseFile;
            form.labelId.Visible = false;
            form.textBoxId.Visible = false;

            //Preenche a tabela
            SampleTools sampleTools = new SampleTools();
            List<decimal> phi = sampleTools.getPhiKeys();
            for (int i = 0; i < phi.Count; i++)
            {
                string key = phi[i].ToString("0.0", CultureInfo.CreateSpecificCulture("PT-BR"));
                form.dataGridView1.Rows.Add(key, "0");
            }
            form.labelPeso.Text = form.dataGridView1.Rows.Cast<DataGridViewRow>().Sum(i => Convert.ToDecimal(i.Cells["Pesos"].Value)).ToString("N2");

            //Preenche as categorias
            foreach (string i in getAllCategories())
            {
                if (i != "")
                {
                    form.comboBoxCategoria.Items.Add(i);
                }
            }

            form.ShowDialog();
        }

        private void EditarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Inicia o form e habilita o campo id
            SampleForm form = new SampleForm();
            form.Owner = this;
            form.MdiParent = MdiParent;
            form.DatabaseFile = DatabaseFile;
            form.labelId.Visible = true;
            form.textBoxId.Visible = true;
            form.buttonSalvar.Text = "Editar";

            //Preenche as categorias
            foreach (string i in getAllCategories())
            {
                form.comboBoxCategoria.Items.Add(i);
            }

            //Obtem o Id selecionado
            string id = null;
            Int32 selectedRowCount = dataGridView.Rows.GetRowCount(DataGridViewElementStates.Selected);
            if(selectedRowCount == 1)
            {
                id = dataGridView.SelectedRows[0].Cells["Id"].Value.ToString();
            }

            //Obtem os dados do banco de dados e preenche a tela para edição da amostra
            if(id != null)
            {
                DatabaseConnect database = new DatabaseConnect(DatabaseFile);
                string sql = "SELECT * FROM Samples WHERE Id=" + id;
                Console.WriteLine(sql);
                SQLiteConnection conn = new SQLiteConnection(database.Connection);
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                SQLiteDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    form.textBoxId.Text = Convert.ToInt32(dr["Id"]).ToString();
                    form.textBoxAmostra.Text = dr["Name"].ToString();
                    form.comboBoxCategoria.Text = dr["Category"].ToString();
                    form.textBoxDescricao.Text = dr["Description"].ToString();
                    string data = dr["Date"].ToString();
                    DateTime oData = Convert.ToDateTime(data);
                    form.dateTimePickerData.Value = oData;

                    form.numericUpDownCarbonato.Value = Convert.ToDecimal(dr["Carbonates"]);
                    form.numericUpDownLatitude.Value = Convert.ToDecimal(dr["Latitude"]);
                    form.numericUpDownLongitude.Value = Convert.ToDecimal(dr["Longitude"]);

                    SampleTools sampleTools = new SampleTools();
                    List<decimal> phi = sampleTools.getPhiKeys();
                    Decimal WeightTotal = 0;
                    for (int i = 0; i < phi.Count; i++)
                    {
                        decimal WeightName = Convert.ToDecimal(dr["Weight" + i.ToString()]);
                        string WeightValue = WeightName.ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR"));
                        string key = phi[i].ToString("0.0", CultureInfo.CreateSpecificCulture("PT-BR"));
                        form.dataGridView1.Rows.Insert(i, key, WeightValue);
                        WeightTotal = WeightTotal + WeightName;
                    }

                    form.labelPeso.Text = WeightTotal.ToString();
                }
                conn.Close();
            }

            //Exibe o form
            if (selectedRowCount > 1)
            {
                MessageBox.Show("Selecione apenas uma linha da tabela para edição.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                form.ShowDialog();
            }

        }

        private void ExcluirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Obtem o Id selecionado
            string id = null;
            Int32 selectedRowCount = dataGridView.Rows.GetRowCount(DataGridViewElementStates.Selected);
            if (selectedRowCount == 1)
            {
                id = dataGridView.SelectedRows[0].Cells["Id"].Value.ToString();
            }
            else
            {
                MessageBox.Show("Selecione apenas uma linha da tabela para exclusão.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //Solicita a confirmação da exclusão
            DialogResult result = MessageBox.Show("Tem certeza que deseja excluir a amostra selecionada?", "Excluir", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (result.Equals(DialogResult.OK))
            {
                if (id != null)
                {
                    DatabaseConnect database = new DatabaseConnect(DatabaseFile);
                    string sql = "DELETE FROM Samples WHERE Id=" + id;
                    Console.WriteLine(sql);
                    SQLiteConnection conn = new SQLiteConnection(database.Connection);
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();
                    SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                    SQLiteDataReader dr = cmd.ExecuteReader();
                    conn.Close();
                    MessageBox.Show("Registro excluído com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    //Atualiza a área de trabalho
                    updateDataGrid(null, null, null, true);
                }
            }

            
        }

        private void InserirNovaLinhaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InserirToolStripMenuItem_Click(sender, e);
        }

        private void EditarToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            EditarToolStripMenuItem_Click(sender, e);
        }

        private void ExluirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExcluirToolStripMenuItem_Click(sender, e);
        }

        private void TreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            string nodeSelected = e.Node.Text;
            if(e.Node.Parent != null)
            {
                if(e.Node.Parent.Text == "Amostras")
                {
                    if (nodeSelected != "Todas" || nodeSelected != "Amostras")
                    {
                        updateDataGrid(nodeSelected, null, null, false);
                    }
                    if (nodeSelected == "Todas" || nodeSelected == "Amostras")
                    {
                        updateDataGrid(null, null, null, false);
                    }
                }

                if (e.Node.Parent.Text == "Categorias")
                {
                    if (nodeSelected != "Todas" || nodeSelected != "Categorias")
                    {
                        updateDataGrid(null, nodeSelected, null, false);
                    }
                    if (nodeSelected == "Todas" || nodeSelected == "Categorias")
                    {
                        updateDataGrid(null, null, null, false);
                    }
                }

                if (e.Node.Parent.Text == "Datas")
                {
                    if (nodeSelected != "Todas" || nodeSelected != "Datas")
                    {
                        updateDataGrid(null, null, nodeSelected, false);
                    }
                    if (nodeSelected == "Todas" || nodeSelected == "Datas")
                    {
                        updateDataGrid(null, null, null, false);
                    }
                }

            }
        }

        private void CopiarCélulasSelecionadasToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CopyDataGridViewToClipboard(dataGridView, false);
        }

        private void CopiarTodaATabelaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyDataGridViewToClipboard(dataGridView, true, true);
        }

        private void CopiarCélulasSelecionadasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopiarCélulasSelecionadasToolStripMenuItem1_Click(sender, e);
        }

        private void CopiarTodaATabelaToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CopiarTodaATabelaToolStripMenuItem_Click(sender, e);
        }

        private void CopyDataGridViewToClipboard(DataGridView dgv, bool includeHeaders = true, bool allRows = false)
        {
            try
            {
                string s = "";
                DataGridViewColumn oCurrentCol = dgv.Columns.GetFirstColumn(DataGridViewElementStates.Visible);
                if (includeHeaders)
                {
                    do
                    {
                        s = s + oCurrentCol.HeaderText + "\t";
                        oCurrentCol = dgv.Columns.GetNextColumn(oCurrentCol, DataGridViewElementStates.Visible, DataGridViewElementStates.None);
                    }
                    while (oCurrentCol != null);
                    s = s.Substring(0, s.Length - 1);
                    s = s + Environment.NewLine;    //Get rows
                }
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    oCurrentCol = dgv.Columns.GetFirstColumn(DataGridViewElementStates.Visible);

                    if (row.Selected || allRows)
                    {
                        do
                        {
                            if (row.Cells[oCurrentCol.Index].Value != null) s = s + row.Cells[oCurrentCol.Index].Value.ToString();
                            s = s + "\t";
                            oCurrentCol = dgv.Columns.GetNextColumn(oCurrentCol, DataGridViewElementStates.Visible, DataGridViewElementStates.None);
                        }
                        while (oCurrentCol != null);
                        s = s.Substring(0, s.Length - 1);
                        s = s + Environment.NewLine;
                    }
                }
                Clipboard.SetText(s);
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Error: " + ex.Message);
            }
        }

        private void ProcessarSelecionadaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ResultSample form = new ResultSample();
                form.Owner = this;
                form.MdiParent = MdiParent;

                //Obtem o Id selecionado
                string id = null;
                Int32 selectedRowCount = dataGridView.Rows.GetRowCount(DataGridViewElementStates.Selected);
                if (selectedRowCount == 1)
                {
                    id = dataGridView.SelectedRows[0].Cells["Id"].Value.ToString();
                }

                //cria a tabela
                int d = 0;
                for (char ch = 'A'; ch <= 'Z'; ch++)
                {
                    int i = (int)ch;
                    DataGridViewColumn dtCol = new DataGridViewColumn();
                    dtCol.Name = ch.ToString();
                    dtCol.DataPropertyName = ch.ToString();
                    dtCol.CellTemplate = new DataGridViewTextBoxCell();
                    dtCol.ReadOnly = true;
                    form.dataGridView1.Columns.Insert(d, dtCol);
                    d++;
                }
                int c = 0;
                while (c < 100)
                {
                    form.dataGridView1.Rows.Insert(c);

                    c++;
                }

                //Obtem os dados do banco de dados e preenche a tela para edição da amostra
                if (id != null)
                {
                    SampleTools sampleTools = new SampleTools();
                    List<decimal> phi = sampleTools.getPhiKeys();
                    List<decimal> dmm = sampleTools.getDmmKeys();

                    DatabaseConnect database = new DatabaseConnect(DatabaseFile);
                    string sql = "SELECT * FROM Samples WHERE Id=" + id;
                    Console.WriteLine(sql);
                    SQLiteConnection conn = new SQLiteConnection(database.Connection);
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();
                    SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                    SQLiteDataReader dr = cmd.ExecuteReader();
                    Sample sample = new Sample();
                    while (dr.Read())
                    {
                        sample.Name = dr["Name"].ToString();
                        sample.Category = dr["Category"].ToString();
                        sample.Date = dr["Date"].ToString();
                        sample.Description = dr["Description"].ToString();
                        sample.Latitude = Convert.ToDecimal(dr["Latitude"]);
                        sample.Longitude = Convert.ToDecimal(dr["Longitude"]);
                        sample.Carbonates = Convert.ToDecimal(dr["Carbonates"]);

                        for (int i = 0; i < phi.Count; i++)
                        {
                            PropertyInfo pinfo = typeof(Sample).GetProperty("Weight" + i);
                            pinfo.SetValue(sample, Convert.ToDecimal(dr["Weight" + i.ToString()]));
                        }

                    }
                    conn.Close();

                    //Insere as informações basicas
                    form.dataGridView1.Rows.Insert(0, "IDENTIFICAÇÃO DA AMOSTRA");
                    form.dataGridView1.Rows.Insert(1, "Amostra", sample.Name.ToString());
                    form.dataGridView1.Rows.Insert(2, "Categoria", sample.Category.ToString());
                    form.dataGridView1.Rows.Insert(3, "Data", sample.Date.ToString());
                    form.dataGridView1.Rows.Insert(4, "Descrição", sample.Description.ToString());
                    form.dataGridView1.Rows.Insert(5, "Latitude", sample.Latitude.ToString("0.00000", CultureInfo.CreateSpecificCulture("PT-BR")));
                    form.dataGridView1.Rows.Insert(6, "Longitude", sample.Longitude.ToString("0.00000", CultureInfo.CreateSpecificCulture("PT-BR")));
                    form.dataGridView1.Rows.Insert(7, "Carbonatos", sample.Carbonates.ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR")));

                    //Calcula o Weight total
                    decimal WeightTotal = sampleTools.getTotalWeight(sample);
                    form.dataGridView1.Rows.Insert(8, "Peso total", WeightTotal.ToString());

                    //Constroi a tabela de frequencias
                    form.dataGridView1.Rows.Insert(10, "FREQUÊNCIAS");
                    form.dataGridView1.Rows.Insert(11, "PHI", "D(mm)", "PESO", "FREQUÊNCIA", "F. ACUMULADA", "Wentworth(1922)"/*, "Friedman(1978)", "Blott(2001)"*/);
                    Decimal frequenciaAcumulada = 0;
                    for (int i = 0; i < phi.Count; i++)
                    {
                        string WeightName = "Weight" + i;
                        PropertyInfo pinfo = typeof(Sample).GetProperty(WeightName);
                        decimal Weight = Convert.ToDecimal(pinfo.GetValue(sample));

                        string WeightValue = Weight.ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR"));
                        string keyPhi = phi[i].ToString("0.0", CultureInfo.CreateSpecificCulture("PT-BR"));
                        string keyDmm = dmm[i].ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR"));

                        Decimal frequencia = (Convert.ToDecimal(Weight) / WeightTotal) * 100;
                        string frequenciaStr = frequencia.ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR"));

                        frequenciaAcumulada = frequencia + frequenciaAcumulada;
                        string frequenciaAcumuladaStr = frequenciaAcumulada.ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR"));

                        string classificationsWentworth = sampleTools.getPhiClassifications("Wentworth(1922)")[i];
                        string classificationsFriedman = sampleTools.getPhiClassifications("Friedman(1978)")[i];
                        string classificationsBlott = sampleTools.getPhiClassifications("Blott(2001)")[i];

                        form.dataGridView1.Rows.Insert(i + 12, keyPhi, keyDmm, WeightValue, frequenciaStr, frequenciaAcumuladaStr, classificationsWentworth/*, classificationsFriedman, classificationsBlott*/);
                    }

                    //Constroi a tabela de percentis
                    form.dataGridView1.Rows.Insert(39, "PERCENTIS");
                    form.dataGridView1.Rows.Insert(40, "phi5", "phi16", "phi25", "phi50", "phi75", "phi84", "phi95");
                    List<decimal> percentis = sampleTools.getPercentis(sample);
                    form.dataGridView1.Rows.Insert(41, 
                        percentis[5].ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR")),
                        percentis[16].ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR")),
                        percentis[25].ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR")),
                        percentis[50].ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR")),
                        percentis[75].ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR")),
                        percentis[84].ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR")),
                        percentis[95].ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR"))
                        );

                    //Constroi a tabela de statísticas
                    List<decimal> statisticsFolk = sampleTools.getStatisticsByMehtod("Folk&Ward(1957)", sample);
                    List<decimal> statisticsMca = sampleTools.getStatisticsByMehtod("McCammonA(1962)", sample);
                    List<decimal> statisticsMcb = sampleTools.getStatisticsByMehtod("McCammonB(1962)", sample);
                    List<decimal> statisticsTask = sampleTools.getStatisticsByMehtod("Trask(1930)", sample);
                    List<decimal> statisticsOtto = sampleTools.getStatisticsByMehtod("Otto(1939)", sample);
                    form.dataGridView1.Rows.Insert(43, "ESTATÍSTICAS");
                    form.dataGridView1.Rows.Insert(44, "Média",                                "Mediana",                              "Selecionamento",                       "Assimetria",                           "Custose");
                    form.dataGridView1.Rows.Insert(45, f.decimalToString(statisticsFolk[0]),   f.decimalToString(statisticsFolk[1]),   f.decimalToString(statisticsFolk[2]),   f.decimalToString(statisticsFolk[3]),   f.decimalToString(statisticsFolk[4]));
                    /*form.dataGridView1.Rows.Insert(46, "McCammonA(1962)",   f.decimalToString(statisticsMca[0]),    f.decimalToString(statisticsMca[1]),    f.decimalToString(statisticsMca[2]),    f.decimalToString(statisticsMca[3]),    f.decimalToString(statisticsMca[4]));
                    form.dataGridView1.Rows.Insert(47, "McCammonB(1962)",   f.decimalToString(statisticsMcb[0]),    f.decimalToString(statisticsMcb[1]),    f.decimalToString(statisticsMcb[2]),    f.decimalToString(statisticsMcb[3]),    f.decimalToString(statisticsMcb[4]));
                    form.dataGridView1.Rows.Insert(48, "Trask(1930)",       f.decimalToString(statisticsTask[0]),   f.decimalToString(statisticsTask[1]),   f.decimalToString(statisticsTask[2]),   f.decimalToString(statisticsTask[3]),   f.decimalToString(statisticsTask[4]));
                    form.dataGridView1.Rows.Insert(49, "Otto(1939)",        f.decimalToString(statisticsOtto[0]),   f.decimalToString(statisticsOtto[1]),   f.decimalToString(statisticsOtto[2]),   f.decimalToString(statisticsOtto[3]),   f.decimalToString(statisticsOtto[4]));*/


                    //Constroi a tabela de classificação
                    List<decimal> frequencies = sampleTools.getFrequencies(sample);
                    form.dataGridView1.Rows.Insert(47, "CLASSIFICAÇÃO PELA FREQUÊNCIA");
                    form.dataGridView1.Rows.Insert(48, "Cascalho",
                        f.decimalToString(frequencies[0]+ frequencies[1]+ frequencies[2]+ frequencies[3]+ frequencies[4]+ frequencies[5]+ frequencies[6])
                        );
                    form.dataGridView1.Rows.Insert(49, "Areia muito grossa",
                        f.decimalToString(frequencies[7] + frequencies[8])
                        );
                    form.dataGridView1.Rows.Insert(50, "Areia grossa",
                        f.decimalToString(frequencies[9] + frequencies[10])
                        );
                    form.dataGridView1.Rows.Insert(51, "Areia média",
                        f.decimalToString(frequencies[11] + frequencies[12])
                        );
                    form.dataGridView1.Rows.Insert(52, "Areia fina",
                        f.decimalToString(frequencies[13] + frequencies[14])
                        );
                    form.dataGridView1.Rows.Insert(53, "Areia muito fina",
                        f.decimalToString(frequencies[15] + frequencies[16])
                        );
                    form.dataGridView1.Rows.Insert(54, "Silte",
                        f.decimalToString(frequencies[17] + frequencies[18] + frequencies[19] + frequencies[20] + frequencies[21])
                        );
                    form.dataGridView1.Rows.Insert(55, "Argila",
                        f.decimalToString(frequencies[22] + frequencies[23] + frequencies[24] + frequencies[25])
                        );

                    form.dataGridView1.Rows.Insert(57, "CLASSIFICAÇÃO VERBAL");
                    form.dataGridView1.Rows.Insert(58, "Classificação pela média",                                        "Selecionamento",                                               "Assimetria", "Custose");
                    form.dataGridView1.Rows.Insert(59, sampleTools.getPhiClassificationSimple(statisticsFolk[0]),    sampleTools.getClassificationBySelection(statisticsFolk[2]),    sampleTools.getClassificationByAssimetry(statisticsFolk[3]), sampleTools.getClassificationByCurtose(statisticsFolk[4]));
                    /*form.dataGridView1.Rows.Insert(60, "McCammonA(1962)", sampleTools.getPhiClassificationSimple(statisticsMca[0]),     sampleTools.getClassificationBySelection(statisticsMca[2]),     sampleTools.getClassificationByAssimetry(statisticsMca[3]), sampleTools.getClassificationByCurtose(statisticsMca[4]));
                    form.dataGridView1.Rows.Insert(61, "McCammonB(1962)", sampleTools.getPhiClassificationSimple(statisticsMcb[0]),     sampleTools.getClassificationBySelection(statisticsMcb[2]),     sampleTools.getClassificationByAssimetry(statisticsMcb[3]), sampleTools.getClassificationByCurtose(statisticsMcb[4]));
                    form.dataGridView1.Rows.Insert(62, "Trask(1930)",     sampleTools.getPhiClassificationSimple(statisticsTask[0]),    sampleTools.getClassificationBySelection(statisticsTask[2]),    sampleTools.getClassificationByAssimetry(statisticsTask[3]), sampleTools.getClassificationByCurtose(statisticsTask[4]));
                    form.dataGridView1.Rows.Insert(63, "Otto(1939)",      sampleTools.getPhiClassificationSimple(statisticsOtto[0]),    sampleTools.getClassificationBySelection(statisticsOtto[2]),    sampleTools.getClassificationByAssimetry(statisticsOtto[3]), sampleTools.getClassificationByCurtose(statisticsOtto[4]));*/

                    //Limpa as céulas vazias da datagrid
                    int r = 0;
                    foreach (DataGridViewRow rw in form.dataGridView1.Rows)
                    {
                        for (int i = 0; i < rw.Cells.Count; i++)
                        {
                            if (rw.Cells[i].Value == null || rw.Cells[i].Value == DBNull.Value || String.IsNullOrWhiteSpace(rw.Cells[i].Value.ToString()))
                            {
                                //MessageBox.Show(i.ToString());
                                form.dataGridView1.Rows[r].Cells[i].Value = "";
                            }
                        }
                        r++;
                    }

                    decimal coquinas = 0;
                    decimal rodolitos = 0;
                    decimal seixos = sample.Weight0 + sample.Weight1 + sample.Weight2 + sample.Weight3 + sample.Weight4;
                    decimal granulos = sample.Weight5 + sample.Weight6;
                    decimal sand = sample.Weight7 + sample.Weight8 + sample.Weight9 + sample.Weight10 + sample.Weight11 + sample.Weight12 + sample.Weight13 + sample.Weight14 + sample.Weight15 + sample.Weight16;
                    decimal lama = sample.Weight17 + sample.Weight18 + sample.Weight19 + sample.Weight20 + sample.Weight21 + sample.Weight22 + sample.Weight23 + sample.Weight24 + sample.Weight25;

                    //FOLK

                    string siglaFolk = sampleTools.getClassificationFolk("sigla", sample);
                    string classificationFolk = sampleTools.getClassificationFolk("classification", sample);

                    form.dataGridView1.Rows.Insert(61, "CLASSIFICAÇÃO DE FOLK");
                    form.dataGridView1.Rows.Insert(62, "REFERÊNCIA", "Sigla", "Classificação Verbal", /*"%Coquinas", "%Rodolitos",*/ "");
                    form.dataGridView1.Rows.Insert(63, "Folk&Ward(1957)", siglaFolk, classificationFolk);




                    //Larsonneur
                    decimal median = statisticsFolk[1];
                    decimal carbonatosP = sample.Carbonates;
                    decimal coquinasP = (100 * coquinas) / WeightTotal;
                    decimal rodolitosP = (100 * rodolitos) / WeightTotal;
                    decimal seixosP = (100 * seixos) / WeightTotal;
                    decimal granulosP = (100 * granulos) / WeightTotal;
                    decimal sandP = (100 * sand) / WeightTotal;
                    decimal lamaP = (100 * lama) / WeightTotal;
                    decimal scrP = 100m - sandP - lamaP;

                    decimal p05a20P = (100 * (sample.Weight7 + sample.Weight8 + sample.Weight9 + sample.Weight10))/WeightTotal;
                    decimal p025a05P = (100 * (sample.Weight11 + sample.Weight12)) / WeightTotal;
                    decimal p005a025P = (100 * (sample.Weight13 + sample.Weight14)) / WeightTotal;

                    string siglaLarsonneur = sampleTools.getClassificationLarsonneur("sigla", sample);
                    string classificationLarsonneur = sampleTools.getClassificationLarsonneur("classification", sample);

                    form.dataGridView1.Rows.Insert(65, "CLASSIFICAÇÃO DE LARSONNEUR");
                    form.dataGridView1.Rows.Insert(66, "REFERÊNCIA", "Sigla", "Classificação Verbal", /*"%Coquinas", "%Rodolitos",*/ "%Seixos", "%Granulos", "%Areias", "%Lama");
                    form.dataGridView1.Rows.Insert(67, "Larsonneur(1977)/Dias(1996)", siglaLarsonneur, classificationLarsonneur, /*f.decimalToString(coquinasP), f.decimalToString(rodolitosP),*/ f.decimalToString(seixosP), f.decimalToString(granulosP), f.decimalToString(sandP), f.decimalToString(lamaP));
                }

                //Exibe o form
                if (selectedRowCount > 1)
                {
                    MessageBox.Show("Selecione apenas uma linha da tabela para edição.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao acessar os dados: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ProcessarVariasAmostras(string method)
        {
            try
            {
                SampleTools sampleTools = new SampleTools();

                ResultSample form = new ResultSample();
                form.Owner = this;
                form.MdiParent = MdiParent;

                //Insere o método no titulo
                form.Text = form.Text + " - " + method;

                //Obtem o Id selecionado
                string id = null;
                Int32 selectedRowCount = dataGridView.Rows.GetRowCount(DataGridViewElementStates.Selected);

                //cria a tabela
                int d = 0;
                for (char ch = 'A'; ch <= 'Z'; ch++)
                {
                    int i = (int)ch;
                    DataGridViewColumn dtCol = new DataGridViewColumn();
                    dtCol.Name = ch.ToString();
                    dtCol.DataPropertyName = ch.ToString();
                    dtCol.CellTemplate = new DataGridViewTextBoxCell();
                    dtCol.ReadOnly = true;
                    form.dataGridView1.Columns.Insert(d, dtCol);
                    d++;
                }
                int c = 0;
                while (c < 100)
                {
                    form.dataGridView1.Rows.Insert(c);

                    c++;
                }

                //Constroi amostra
                form.dataGridView1.Rows.Insert(0, "Id", "Amostra", "Categoria", "Data", "Latitude", "Longitude", "Peso total", "Media", "Mediana", "Seleção", "Assimetria", "Curtose", "Classificação pela média", "Classificação pelo Selecionamento", "Classificação pela Assimetria", "Classificação pela Custose", "Larsonneur(1977)/Dias(1996)", "phi5", "phi16", "phi25", "phi50", "phi75", "phi84", "phi95");

                /*form.dataGridView1.Columns[0].HeaderText = "Id";
                form.dataGridView1.Columns[1].HeaderText = "Amostra";
                form.dataGridView1.Columns[2].HeaderText = "Categori1a";
                form.dataGridView1.Columns[3].HeaderText = "Data";
                form.dataGridView1.Columns[4].HeaderText = "Media";
                form.dataGridView1.Columns[5].HeaderText = "Mediana";
                form.dataGridView1.Columns[6].HeaderText = "Seleção";
                form.dataGridView1.Columns[7].HeaderText = "Assimetria";
                form.dataGridView1.Columns[8].HeaderText = "Curtose";
                form.dataGridView1.Columns[9].HeaderText = "Classificação pela média";
                form.dataGridView1.Columns[10].HeaderText = "Classificação pelo Selecionamento";
                form.dataGridView1.Columns[11].HeaderText = "Classificação pela Assimetria";
                form.dataGridView1.Columns[12].HeaderText = "Classificação pela Custose";
                form.dataGridView1.Columns[13].HeaderText = "phi5";
                form.dataGridView1.Columns[14].HeaderText = "phi16";
                form.dataGridView1.Columns[15].HeaderText = "phi25";
                form.dataGridView1.Columns[16].HeaderText = "phi50";
                form.dataGridView1.Columns[17].HeaderText = "phi75";
                form.dataGridView1.Columns[18].HeaderText = "phi84";
                form.dataGridView1.Columns[19].HeaderText = "phi95";*/


                Sample sample = new Sample();

                DataGridViewSelectedRowCollection z = dataGridView.SelectedRows;
                int u = 0;
                foreach (DataGridViewRow row in dataGridView.SelectedRows.Cast<DataGridViewRow>().Reverse())
                {
                    id = row.Cells["Id"].Value.ToString();

                    if (id != null)
                    {
                        List<decimal> phi = sampleTools.getPhiKeys();
                        List<decimal> dmm = sampleTools.getDmmKeys();

                        DatabaseConnect database = new DatabaseConnect(DatabaseFile);
                        string sql = "SELECT * FROM Samples WHERE Id=" + id;
                        Console.WriteLine(sql);
                        SQLiteConnection conn = new SQLiteConnection(database.Connection);
                        if (conn.State == ConnectionState.Closed)
                            conn.Open();
                        SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                        SQLiteDataReader dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            sample.Id = Convert.ToInt32(dr["Id"]);
                            sample.Name = dr["Name"].ToString();
                            sample.Category = dr["Category"].ToString();
                            sample.Date = dr["Date"].ToString();
                            sample.Description = dr["Description"].ToString();
                            sample.Latitude = Convert.ToDecimal(dr["Latitude"]);
                            sample.Longitude = Convert.ToDecimal(dr["Longitude"]);
                            sample.Carbonates = Convert.ToDecimal(dr["Carbonates"]);

                            for (int i = 0; i < phi.Count; i++)
                            {
                                PropertyInfo pinfo = typeof(Sample).GetProperty("Weight" + i);
                                pinfo.SetValue(sample, Convert.ToDecimal(dr["Weight" + i.ToString()]));
                            }

                        }
                        conn.Close();
                    }

                    decimal media = 0;
                    decimal mediana = 0;
                    decimal selection = 0;
                    decimal assimetry = 0;
                    decimal curtose = 0;
                    string cmedia = "";
                    string cselection = "";
                    string cassimetry = "";
                    string ccurtose = "";

                    List<decimal> statisticsFolk = sampleTools.getStatisticsByMehtod("Folk&Ward(1957)", sample);
                    List<decimal> statisticsMca = sampleTools.getStatisticsByMehtod("McCammonA(1962)", sample);
                    List<decimal> statisticsMcb = sampleTools.getStatisticsByMehtod("McCammonB(1962)", sample);
                    List<decimal> statisticsTask = sampleTools.getStatisticsByMehtod("Trask(1930)", sample);
                    List<decimal> statisticsOtto = sampleTools.getStatisticsByMehtod("Otto(1939)", sample);

                    //Insere as de classificação
                    if(method == "Folk&Ward(1957)")
                    {
                        media = statisticsFolk[0];
                        mediana = statisticsFolk[1];
                        selection = statisticsFolk[2];
                        assimetry = statisticsFolk[3];
                        curtose = statisticsFolk[4];
                        cmedia = sampleTools.getPhiClassificationSimple(statisticsFolk[0]);
                        cselection = sampleTools.getClassificationBySelection(statisticsFolk[2]);
                        cassimetry = sampleTools.getClassificationByAssimetry(statisticsFolk[3]);
                        ccurtose = sampleTools.getClassificationByCurtose(statisticsFolk[4]);
                    }
                    if (method == "McCammonA(1962)")
                    {
                        media = statisticsMca[0];
                        mediana = statisticsMca[1];
                        selection = statisticsMca[2];
                        assimetry = statisticsMca[3];
                        curtose = statisticsMca[4];
                        cmedia = sampleTools.getPhiClassificationSimple(statisticsMca[0]);
                        cselection = sampleTools.getClassificationBySelection(statisticsMca[2]);
                        cassimetry = sampleTools.getClassificationByAssimetry(statisticsMca[3]);
                        ccurtose = sampleTools.getClassificationByCurtose(statisticsMca[4]);
                    }
                    if (method == "McCammonB(1962)")
                    {
                        media = statisticsMcb[0];
                        mediana = statisticsMcb[1];
                        selection = statisticsMcb[2];
                        assimetry = statisticsMcb[3];
                        curtose = statisticsMcb[4];
                        cmedia = sampleTools.getPhiClassificationSimple(statisticsMcb[0]);
                        cselection = sampleTools.getClassificationBySelection(statisticsMcb[2]);
                        cassimetry = sampleTools.getClassificationByAssimetry(statisticsMcb[3]);
                        ccurtose = sampleTools.getClassificationByCurtose(statisticsMcb[4]);
                    }
                    if (method == "Trask(1930)")
                    {
                        media = statisticsTask[0];
                        mediana = statisticsTask[1];
                        selection = statisticsTask[2];
                        assimetry = statisticsTask[3];
                        curtose = statisticsTask[4];
                        cmedia = sampleTools.getPhiClassificationSimple(statisticsTask[0]);
                        cselection = sampleTools.getClassificationBySelection(statisticsTask[2]);
                        cassimetry = sampleTools.getClassificationByAssimetry(statisticsTask[3]);
                        ccurtose = sampleTools.getClassificationByCurtose(statisticsTask[4]);
                    }
                    if (method == "Otto(1939)")
                    {
                        media = statisticsOtto[0];
                        mediana = statisticsOtto[1];
                        selection = statisticsOtto[2];
                        assimetry = statisticsOtto[3];
                        curtose = statisticsOtto[4];
                        cmedia = sampleTools.getPhiClassificationSimple(statisticsOtto[0]);
                        cselection = sampleTools.getClassificationBySelection(statisticsOtto[2]);
                        cassimetry = sampleTools.getClassificationByAssimetry(statisticsOtto[3]);
                        ccurtose = sampleTools.getClassificationByCurtose(statisticsOtto[4]);
                    }

                    //Constroi a tabela de percentis
                    List<decimal> percentis = sampleTools.getPercentis(sample);

                    //Classificação de Larsonneur
                    string sigla = sampleTools.getClassificationLarsonneur("sigla", sample);
                    string LarsonneurClassification = sampleTools.getClassificationLarsonneur("classification", sample);

                    //insere a linha
                    form.dataGridView1.Rows.Insert(u + 1,
                        sample.Id.ToString(),
                        sample.Name.ToString(),
                        sample.Category.ToString(),
                        sample.Date.ToString(),
                        sample.Latitude,
                        sample.Longitude,
                        sampleTools.getTotalWeight(sample),
                        f.decimalToString(media),
                        f.decimalToString(mediana),
                        f.decimalToString(selection),
                        f.decimalToString(assimetry),
                        f.decimalToString(curtose),
                        cmedia,
                        cselection,
                        cassimetry,
                        ccurtose,
                        sigla + " - " + LarsonneurClassification,
                        percentis[5].ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR")),
                        percentis[16].ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR")),
                        percentis[25].ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR")),
                        percentis[50].ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR")),
                        percentis[75].ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR")),
                        percentis[84].ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR")),
                        percentis[95].ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR"))
                    );

                    //Limpa as céulas vazias da datagrid
                    int r = 0;
                    foreach (DataGridViewRow rw in form.dataGridView1.Rows)
                    {
                        for (int i = 0; i < rw.Cells.Count; i++)
                        {
                            if (rw.Cells[i].Value == null || rw.Cells[i].Value == DBNull.Value || String.IsNullOrWhiteSpace(rw.Cells[i].Value.ToString()))
                            {
                                //MessageBox.Show(i.ToString());
                                form.dataGridView1.Rows[r].Cells[i].Value = "";
                            }
                        }
                        r++;
                    }

                    u++;
                }


                //Exibe o form
                if (selectedRowCount < 1)
                {
                    MessageBox.Show("Nenhuma linha da tabela foi selecionada para edição.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao acessar os dados: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FolkWard1967ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessarVariasAmostras("Folk&Ward(1957)");
        }

        private void McCammonA1962ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessarVariasAmostras("McCammonA(1962)");
        }

        private void McCammonB1962ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessarVariasAmostras("McCammonB(1962)");
        }

        private void Trask1930ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessarVariasAmostras("Trask(1930)");
        }

        private void Otto1939ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessarVariasAmostras("Otto(1939)");
        }

        private void HistogramaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ResultChart form = new ResultChart();

                form.Type = "histogram";

                //Serie 0
                var dataPointSeries = new System.Windows.Forms.DataVisualization.Charting.Series
                {
                    Name = "histogram",
                    Color = Color.DodgerBlue,
                    IsVisibleInLegend = true,
                    ChartType = SeriesChartType.Column,
                    MarkerStyle = MarkerStyle.None,
                    BorderWidth = 0,
                    BorderColor = Color.White
                };

                //Obtem o Id selecionado
                string id = null;
                Int32 selectedRowCount = dataGridView.Rows.GetRowCount(DataGridViewElementStates.Selected);
                if (selectedRowCount == 1)
                {
                    id = dataGridView.SelectedRows[0].Cells["Id"].Value.ToString();
                }

                //Obtem os dados do banco de dados e preenche a tela para edição da amostra
                if (id != null)
                {
                    //Cria estrutura da tabela de dados
                    DataGridViewColumn dtColx = new DataGridViewColumn();
                    dtColx.Name = 
                    dtColx.DataPropertyName = "X";
                    dtColx.CellTemplate = new DataGridViewTextBoxCell();
                    dtColx.ReadOnly = true;
                    dtColx.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    form.dataGridView1.Columns.Insert(0, dtColx);
                    DataGridViewColumn dtColy = new DataGridViewColumn();
                    dtColy.Name = 
                    dtColy.DataPropertyName = "Y";
                    dtColy.CellTemplate = new DataGridViewTextBoxCell();
                    dtColy.ReadOnly = true;
                    dtColy.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    form.dataGridView1.Columns.Insert(1, dtColy);

                    //Processa
                    SampleTools sampleTools = new SampleTools();
                    List<decimal> phi = sampleTools.getPhiKeys();
                    List<decimal> dmm = sampleTools.getDmmKeys();
                    DatabaseConnect database = new DatabaseConnect(DatabaseFile);
                    string sql = "SELECT * FROM Samples WHERE Id=" + id;
                    Console.WriteLine(sql);
                    SQLiteConnection conn = new SQLiteConnection(database.Connection);
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();
                    SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                    SQLiteDataReader dr = cmd.ExecuteReader();
                    Sample sample = new Sample();
                    while (dr.Read())
                    {
                        sample.Name = dr["Name"].ToString();
                        sample.Category = dr["Category"].ToString();
                        sample.Date = dr["Date"].ToString();
                        sample.Description = dr["Description"].ToString();
                        sample.Latitude = Convert.ToDecimal(dr["Latitude"]);
                        sample.Longitude = Convert.ToDecimal(dr["Longitude"]);
                        sample.Carbonates = Convert.ToDecimal(dr["Carbonates"]);
                        for (int i = 0; i < phi.Count; i++)
                        {
                            PropertyInfo pinfo = typeof(Sample).GetProperty("Weight" + i);
                            pinfo.SetValue(sample, Convert.ToDecimal(dr["Weight" + i.ToString()]));
                        }
                    }
                    conn.Close();
                    //Calcula o Weight total
                    decimal WeightTotal = sampleTools.getTotalWeight(sample);
                    //Constroi a tabela de frequencias
                    int c = 2;
                    for (int i = 0; i < phi.Count; i++)
                    {
                        string WeightName = "Weight" + i;
                        PropertyInfo pinfo = typeof(Sample).GetProperty(WeightName);
                        decimal Weight = Convert.ToDecimal(pinfo.GetValue(sample));
                        string keyDmm = dmm[i].ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR"));
                        Decimal frequencia = (Convert.ToDecimal(Weight) / WeightTotal) * 100;
                        //Adiciona os dados ao gráfico
                        dataPointSeries.Points.Add(Convert.ToDouble(frequencia));
                        //Substitui os labels do eixo x
                        string keyPhi =  phi[i].ToString();
                        
                        CustomLabel label = new CustomLabel
                        {
                            ToPosition = c,
                            Text = keyPhi
                        };
                        form.chart1.ChartAreas[0].AxisX.CustomLabels.Add(label);
                        //Adiciona dados para a tabela
                        form.dataGridView1.Rows.Insert(i, f.decimalToString(phi[i], 1), f.decimalToString(frequencia));
                        c = c + 2;
                    }
                }
                form.chart1.Series.Add(dataPointSeries);

                //Exibe o form
                if (selectedRowCount > 1)
                {
                    MessageBox.Show("Selecione apenas uma linha da tabela.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao acessar os dados: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrequênciaAcumuladaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ResultChart form = new ResultChart();

                form.Type = "frequencyAcc";

                List<decimal> phi = sampleTools.getPhiKeys();
                List<decimal> dmm = sampleTools.getDmmKeys();

                //Cria estrutura da tabela de dados
                DataGridViewColumn dtColx = new DataGridViewColumn();
                dtColx.Name =
                dtColx.DataPropertyName = "X";
                dtColx.CellTemplate = new DataGridViewTextBoxCell();
                dtColx.ReadOnly = true;
                dtColx.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                form.dataGridView1.Columns.Insert(0, dtColx);
                for (int i = 0; i < phi.Count; i++)
                {
                    form.dataGridView1.Rows.Insert(i, f.decimalToString(phi[i], 1));
                }


                //Processa
                string id = null;
                Int32 selectedRowCount = dataGridView.Rows.GetRowCount(DataGridViewElementStates.Selected);
                for (int r = 0; r < selectedRowCount; r++)
                {
                    //Obtem o id selecionado
                    id = dataGridView.SelectedRows[r].Cells["Id"].Value.ToString();

                    //Obtem os dados do banco de dados e preenche a tela para edição da amostra
                    decimal[] x = new decimal[26];
                    decimal[] y = new decimal[26];
                    if (id != null)
                    {
                        SampleTools sampleTools = new SampleTools();
                    

                        DatabaseConnect database = new DatabaseConnect(DatabaseFile);
                        string sql = "SELECT * FROM Samples WHERE Id=" + id;
                        Console.WriteLine(sql);
                        SQLiteConnection conn = new SQLiteConnection(database.Connection);
                        if (conn.State == ConnectionState.Closed)
                            conn.Open();
                        SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                        SQLiteDataReader dr = cmd.ExecuteReader();
                        Sample sample = new Sample();
                        int c = 1;
                        while (dr.Read())
                        {
                            sample.Name = dr["Name"].ToString();
                            sample.Category = dr["Category"].ToString();
                            sample.Date = dr["Date"].ToString();
                            sample.Description = dr["Description"].ToString();
                            sample.Latitude = Convert.ToDecimal(dr["Latitude"]);
                            sample.Longitude = Convert.ToDecimal(dr["Longitude"]);
                            sample.Carbonates = Convert.ToDecimal(dr["Carbonates"]);
                            for (int i = 0; i < phi.Count; i++)
                            {
                                PropertyInfo pinfo = typeof(Sample).GetProperty("Weight" + i);
                                pinfo.SetValue(sample, Convert.ToDecimal(dr["Weight" + i.ToString()]));
                            }

                            //Constroi colunas da tabela de dados
                            DataGridViewColumn dtColy = new DataGridViewColumn();
                            dtColy.Name =
                            dtColy.DataPropertyName = "Y-"+sample.Name;
                            dtColy.CellTemplate = new DataGridViewTextBoxCell();
                            dtColy.ReadOnly = true;
                            dtColy.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                            form.dataGridView1.Columns.Insert(c, dtColy);
                            c++;
                        }
                        conn.Close();

                        //Calcula o Weight total
                        decimal WeightTotal = sampleTools.getTotalWeight(sample);

                        //Constroi a tabela de frequencias
                        Decimal frequenciaAcumuladaAnterior = 0;
                        Decimal frequenciaAcumulada = 0;
                        for (int i = 0; i < phi.Count; i++)
                        {
                            string WeightName = "Weight" + i;
                            PropertyInfo pinfo = typeof(Sample).GetProperty(WeightName);
                            decimal Weight = Convert.ToDecimal(pinfo.GetValue(sample));
                            string keyDmm = dmm[i].ToString("0.000", CultureInfo.CreateSpecificCulture("PT-BR"));
                            Decimal frequencia = (Convert.ToDecimal(Weight) / WeightTotal) * 100;
                            frequenciaAcumuladaAnterior = frequenciaAcumulada;
                            frequenciaAcumulada = frequencia + frequenciaAcumulada;
                            x[i] = phi[i];
                            y[i] = frequenciaAcumulada;
                            //Adiciona dados para a tabela
                            //form.dataGridView1.Rows.Insert(i, f.decimalToString(phi[i], 1), f.decimalToString(frequenciaAcumulada));
                            form.dataGridView1.Rows[i].Cells[1].Value = f.decimalToString(frequenciaAcumulada);

                        }

                        //Serie 0
                        form.chart1.Series.Add(new Series());

                        form.chart1.Series[r].LegendText = GetRotules(sample);

                        form.chart1.Series[r].ChartType = SeriesChartType.Line;
                        form.chart1.Series[r].IsValueShownAsLabel = false;
                        form.chart1.Series[r].Points.DataBindXY(x, y);
                    }

                

                    //Rename frequency labels
                    for (int d = 0; d <= 10; d++)
                    {
                        CustomLabel label = new CustomLabel
                        {
                            ToPosition = d * 20,
                            Text = d * 10 + "%"
                        };
                        form.chart1.ChartAreas[0].AxisY.CustomLabels.Add(label);

                    }

                }            

                //Exibe o form
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao acessar os dados: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GráficoToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            HistogramaToolStripMenuItem_Click(sender, e);

        }

        public void getThernalGraphic(string formType = "sherpard")
        {
            try
            {
                ResultChart form = new ResultChart();

                form.Type = formType;

                //Cria estrutura da tabela de dados
                DataGridViewColumn dtColz = new DataGridViewColumn();
                dtColz.Name =
                dtColz.DataPropertyName = "Amostra";
                dtColz.CellTemplate = new DataGridViewTextBoxCell();
                dtColz.ReadOnly = true;
                dtColz.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                form.dataGridView1.Columns.Insert(0, dtColz);
                DataGridViewColumn dtColx = new DataGridViewColumn();
                dtColx.Name =
                dtColx.DataPropertyName = "X";
                dtColx.CellTemplate = new DataGridViewTextBoxCell();
                dtColx.ReadOnly = true;
                dtColx.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                form.dataGridView1.Columns.Insert(1, dtColx);
                DataGridViewColumn dtColy = new DataGridViewColumn();
                dtColy.Name =
                dtColy.DataPropertyName = "Y";
                dtColy.CellTemplate = new DataGridViewTextBoxCell();
                dtColy.ReadOnly = true;
                dtColy.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                form.dataGridView1.Columns.Insert(2, dtColy);

                //Obtem o Id selecionado
                string id = null;
                Int32 selectedRowCount = dataGridView.Rows.GetRowCount(DataGridViewElementStates.Selected);
                if (selectedRowCount == 1)
                {
                    id = dataGridView.SelectedRows[0].Cells["Id"].Value.ToString();
                }
                decimal[] x = new decimal[selectedRowCount];
                decimal[] y = new decimal[selectedRowCount];
                string[] l = new string[selectedRowCount];
                for (int r = 0; r < selectedRowCount; r++)
                {
                    //Obtem o id selecionado
                    id = dataGridView.SelectedRows[r].Cells["Id"].Value.ToString();

                    decimal sand = 0;
                    decimal silte = 0;
                    decimal clay = 0;
                    decimal gravel = 0;
                    decimal sludge = 0;

                    if (id != null)
                    {
                        SampleTools sampleTools = new SampleTools();
                        List<decimal> phi = sampleTools.getPhiKeys();
                        List<decimal> dmm = sampleTools.getDmmKeys();

                        DatabaseConnect database = new DatabaseConnect(DatabaseFile);
                        string sql = "SELECT * FROM Samples WHERE Id=" + id;
                        Console.WriteLine(sql);
                        SQLiteConnection conn = new SQLiteConnection(database.Connection);
                        if (conn.State == ConnectionState.Closed)
                            conn.Open();
                        SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                        SQLiteDataReader dr = cmd.ExecuteReader();
                        Sample sample = new Sample();
                        while (dr.Read())
                        {
                            sample.Name = dr["Name"].ToString();
                            sample.Category = dr["Category"].ToString();
                            sample.Date = dr["Date"].ToString();
                            sample.Description = dr["Description"].ToString();
                            sample.Latitude = Convert.ToDecimal(dr["Latitude"]);
                            sample.Longitude = Convert.ToDecimal(dr["Longitude"]);
                            sample.Carbonates = Convert.ToDecimal(dr["Carbonates"]);
                            for (int i = 0; i < phi.Count; i++)
                            {
                                PropertyInfo pinfo = typeof(Sample).GetProperty("Weight" + i);
                                pinfo.SetValue(sample, Convert.ToDecimal(dr["Weight" + i.ToString()]));
                            }

                            //Obtem os valores de areia, silte e argila
                            List<decimal> frequencies = sampleTools.getFrequencies(sample);
                            gravel = frequencies[0] + frequencies[1] + frequencies[2] + frequencies[3] + frequencies[4] + frequencies[5] + frequencies[6];
                            sand = frequencies[7] + frequencies[8] + frequencies[9] + frequencies[10] + frequencies[11] + frequencies[12] + frequencies[13] + frequencies[14] + frequencies[15] + frequencies[16];
                            silte = frequencies[17] + frequencies[18] + frequencies[19] + frequencies[20] + frequencies[21];
                            clay = frequencies[22] + frequencies[23] + frequencies[24] + frequencies[25];
                            sludge = silte + clay;

                            //Preenche os dados para o gráfico
                            decimal total = sand + silte + clay;
                            decimal sand1 = (sand / total) * 100;
                            decimal silte1 = (silte / total) * 100;
                            decimal clay1 = (clay / total) * 100;

                            if (formType == "sherpard" || formType == "pejrup")
                            {
                                decimal y1 = clay1;
                                decimal x1 = silte1;
                                decimal x2 = y1 * (x1 - 50) / 100;
                                decimal x3 = x1 - x2;
                                x[r] = x3 + 10;
                                y[r] = y1 + 10;
                                l[r] = GetRotules(sample);
                            }

                            if(formType == "folk")
                            {
                                decimal y1 = sand1;
                                decimal x1 = silte1;
                                decimal x2 = y1 * (x1 - 50) / 100;
                                decimal x3 = x1 - x2;
                                x[r] = x3 + 10;
                                y[r] = y1 + 10;
                                l[r] = GetRotules(sample);
                            }

                            if(formType == "folkThicks")
                            {
                                total = gravel + sand + sludge;
                                decimal gravelT = (gravel / total) * 100;
                                decimal sandT = (sand / total) * 100;
                                decimal sludgeT = (sludge / total) * 100;

                                decimal y1 = gravelT;
                                decimal x1 = sandT;
                                decimal x2 = y1 * (x1 - 50) / 100;
                                decimal x3 = x1 - x2;
                                x[r] = x3 + 10;
                                y[r] = y1 + 10;
                                l[r] = GetRotules(sample);
                            }

                            form.dataGridView1.Rows.Insert(r, sample.Name, f.decimalToString(x[r]), f.decimalToString(y[r]));

                            
                        }
                        conn.Close();
                    }
                }

                //Serie 0
                form.chart1.Series[0].LegendText = "Sherpard";
                form.chart1.Series[0].ChartType = SeriesChartType.Point;
                form.chart1.Series[0].IsValueShownAsLabel = false;
                form.chart1.Series[0].Points.DataBindXY(x, y);

                //Labels pontos
                for (int d = 0; d < l.Length; d++)
                {
                    form.chart1.Series[0].Points[d].Label = l[d];
                    //form.chart1.Series[0].Points[d].label;
                }

                //Exibe o form
                if (selectedRowCount > 1000)
                {
                    MessageBox.Show("Não é possível processar mais de 1000 amostras por vez.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao acessar os dados: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private string GetRotules(Sample sample)
        {
            IniFile ini = new IniFile(this.SettingsFile);
            if (ini.Read("ROTULES") == "category")
                return sample.Category;
            if (ini.Read("ROTULES") == "description")
                return sample.Description;
            if (ini.Read("ROTULES") == "date")
                return sample.Date;

            return sample.Name;
        }

        private void ShepardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getThernalGraphic("sherpard");
        }

        private void PejrupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getThernalGraphic("pejrup");
        }

        private void ArquivoToolStripMenuItem_MouseHover(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).ForeColor = Color.Black;
        }

        private void ArquivoToolStripMenuItem_MouseLeave(object sender, EventArgs e)
        {
            if(fileMenuOpened == false)
                ((ToolStripMenuItem)sender).ForeColor = Color.White;
        }

        private void ArquivoToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).ForeColor = Color.Black;
        }

        private void ArquivoToolStripMenuItem_DropDownClosed(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).ForeColor = Color.White;
            fileMenuOpened = false;
        }

        private void ArquivoToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            fileMenuOpened = true;
        }

        private void MostrarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (panel1.Visible)
            {
                panel1.Visible = false;
                mostrarToolStripMenuItem.Text = "↓";
            }
            else
            {
                panel1.Visible = true;
                mostrarToolStripMenuItem.Text = "↑";
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            CopiarTodaATabelaToolStripMenuItem_Click(sender, e);
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            ProcessárVáriasAmostrasToolStripMenuItem_Click(sender, e);
            //contextMenuStrip2.Show(button6, new System.Drawing.Point(0, button6.Height));
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            DiagramaDeFolkToolStripMenuItem1_Click(sender, e);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            CopiarCélulasSelecionadasToolStripMenuItem1_Click(sender, e);
        }

        private void Button14_Click(object sender, EventArgs e)
        {
            InserirToolStripMenuItem_Click(sender, e);
        }

        private void Button13_Click(object sender, EventArgs e)
        {
            EditarToolStripMenuItem_Click(sender, e);
        }

        private void Button12_Click(object sender, EventArgs e)
        {
            ExcluirToolStripMenuItem_Click(sender, e);
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            ProcessarSelecionadaToolStripMenuItem_Click(sender, e);
        }

        private void Button8_Click(object sender, EventArgs e)
        {
            HistogramaToolStripMenuItem_Click(sender, e);
        }

        private void Button9_Click(object sender, EventArgs e)
        {
            FrequênciaAcumuladaToolStripMenuItem_Click(sender, e);
        }

        private void Button10_Click(object sender, EventArgs e)
        {
            ShepardToolStripMenuItem_Click(sender, e);
        }

        private void Button11_Click(object sender, EventArgs e)
        {
            PejrupToolStripMenuItem_Click(sender, e);
        }

        private void FolkEWard1967ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolkWard1967ToolStripMenuItem_Click(sender, e);
        }

        private void McCammonA1962ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            McCammonA1962ToolStripMenuItem_Click(sender, e);
        }

        private void McCammonB1962ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            McCammonB1962ToolStripMenuItem_Click(sender, e);
        }

        private void Trask1930ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Trask1930ToolStripMenuItem_Click(sender, e);
        }

        private void Otto1939ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Otto1939ToolStripMenuItem_Click(sender, e);
        }

        private void ExibirAjudaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/foredunes/sigran");
        }

        private void ProcessárVáriasAmostrasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessarVariasAmostras("Folk&Ward(1957)");
        }

        private void DiagramaDeFolkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void DiagramaDeFolkToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            getThernalGraphic("folk");
        }

        private void DiagramaDeFolkGrosseirosToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            getThernalGraphic("folkThicks");
        }

        public void GenerateBivariateChart(string abysses, string ordered)
        {
            string abyssesTitle = "";
            if (abysses == "mean")
                abyssesTitle = "Média";
            if (abysses == "median")
                abyssesTitle = "Mediana";
            if (abysses == "selection")
                abyssesTitle = "Selecionamento";
            if (abysses == "assimetry")
                abyssesTitle = "Assimetria";
            if (abysses == "curtose")
                abyssesTitle = "Curtose";

            string orderedTitle = "";
            if (ordered == "mean")
                orderedTitle = "Média";
            if (ordered == "median")
                orderedTitle = "Mediana";
            if (ordered == "selection")
                orderedTitle = "Selecionamento";
            if (ordered == "assimetry")
                orderedTitle = "Assimetria";
            if (ordered == "curtose")
                orderedTitle = "Curtose";



            string formType = "bivariate";
            try
            {
                ResultChart form = new ResultChart();

                form.Type = formType;

                //Cria estrutura da tabela de dados
                DataGridViewColumn dtColz = new DataGridViewColumn();
                dtColz.Name =
                dtColz.DataPropertyName = "Amostra";
                dtColz.CellTemplate = new DataGridViewTextBoxCell();
                dtColz.ReadOnly = true;
                dtColz.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                form.dataGridView1.Columns.Insert(0, dtColz);
                DataGridViewColumn dtColx = new DataGridViewColumn();
                dtColx.Name =
                dtColx.DataPropertyName = "X";
                dtColx.CellTemplate = new DataGridViewTextBoxCell();
                dtColx.ReadOnly = true;
                dtColx.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                form.dataGridView1.Columns.Insert(1, dtColx);
                DataGridViewColumn dtColy = new DataGridViewColumn();
                dtColy.Name =
                dtColy.DataPropertyName = "Y";
                dtColy.CellTemplate = new DataGridViewTextBoxCell();
                dtColy.ReadOnly = true;
                dtColy.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                form.dataGridView1.Columns.Insert(2, dtColy);

                //Obtem o Id selecionado
                string id = null;
                Int32 selectedRowCount = dataGridView.Rows.GetRowCount(DataGridViewElementStates.Selected);
                if (selectedRowCount == 1)
                {
                    id = dataGridView.SelectedRows[0].Cells["Id"].Value.ToString();
                }
                decimal[] x = new decimal[selectedRowCount];
                decimal[] y = new decimal[selectedRowCount];
                string[] l = new string[selectedRowCount];
                for (int r = 0; r < selectedRowCount; r++)
                {
                    //Obtem o id selecionado
                    id = dataGridView.SelectedRows[r].Cells["Id"].Value.ToString();

                    if (id != null)
                    {
                        SampleTools sampleTools = new SampleTools();
                        List<decimal> phi = sampleTools.getPhiKeys();
                        List<decimal> dmm = sampleTools.getDmmKeys();

                        DatabaseConnect database = new DatabaseConnect(DatabaseFile);
                        string sql = "SELECT * FROM Samples WHERE Id=" + id;
                        Console.WriteLine(sql);
                        SQLiteConnection conn = new SQLiteConnection(database.Connection);
                        if (conn.State == ConnectionState.Closed)
                            conn.Open();
                        SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                        SQLiteDataReader dr = cmd.ExecuteReader();
                        Sample sample = new Sample();
                        while (dr.Read())
                        {
                            sample.Name = dr["Name"].ToString();
                            sample.Category = dr["Category"].ToString();
                            sample.Date = dr["Date"].ToString();
                            sample.Description = dr["Description"].ToString();
                            sample.Latitude = Convert.ToDecimal(dr["Latitude"]);
                            sample.Longitude = Convert.ToDecimal(dr["Longitude"]);
                            sample.Carbonates = Convert.ToDecimal(dr["Carbonates"]);
                            for (int i = 0; i < phi.Count; i++)
                            {
                                PropertyInfo pinfo = typeof(Sample).GetProperty("Weight" + i);
                                pinfo.SetValue(sample, Convert.ToDecimal(dr["Weight" + i.ToString()]));
                            }

                            //Obtem média, mediana, selecionamento, assimetria e curtose
                            decimal media = 0;
                            decimal mediana = 0;
                            decimal selection = 0;
                            decimal assimetry = 0;
                            decimal curtose = 0;

                            List<decimal> statisticsFolk = sampleTools.getStatisticsByMehtod("Folk&Ward(1957)", sample);

                            media = statisticsFolk[0];
                            mediana = statisticsFolk[1];
                            selection = statisticsFolk[2];
                            assimetry = statisticsFolk[3];
                            curtose = statisticsFolk[4];

                            //Insere os valores na lista
                            if (abysses == "mean")
                                x[r] = media;
                            if (abysses == "median")
                                x[r] = mediana;
                            if (abysses == "selection")
                                x[r] = selection;
                            if (abysses == "assimetry")
                                x[r] = assimetry;
                            if (abysses == "curtose")
                                x[r] = curtose;

                            if (ordered == "mean")
                                y[r] = media;
                            if (ordered == "median")
                                y[r] = mediana;
                            if (ordered == "selection")
                                y[r] = selection;
                            if (ordered == "assimetry")
                                y[r] = assimetry;
                            if (ordered == "curtose")
                                y[r] = curtose;

                            l[r] = GetRotules(sample);

                            form.dataGridView1.Rows.Insert(r, l[r], f.decimalToString(x[r]), f.decimalToString(y[r]));

                        }
                        conn.Close();
                    }
                }

                //Serie 0
                form.chart1.Series[0].ChartType = SeriesChartType.Point;
                form.chart1.Series[0].IsValueShownAsLabel = false;
                form.chart1.Series[0].Points.DataBindXY(x, y);

                //Labels pontos
                for (int d = 0; d < l.Length; d++)
                {
                    form.chart1.Series[0].Points[d].Label = l[d];
                }

                //Rename titles
                form.chart1.ChartAreas[0].AxisX.Title = abyssesTitle;
                form.chart1.ChartAreas[0].AxisY.Title = orderedTitle;
                Title title = form.chart1.Titles.Add(abyssesTitle + " X " + orderedTitle);
                title.Font = new System.Drawing.Font("Arial", 16, FontStyle.Bold);
                title.ForeColor = System.Drawing.Color.Black;

                //Exibe o form
                if (selectedRowCount > 100)
                {
                    MessageBox.Show("Não é possível processar mais de 100 amostras por vez.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao acessar os dados: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MédiaXSelecionamentoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GenerateBivariateChart("mean", "selection");
        }

        private void MédiaXAssimetriaToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GenerateBivariateChart("mean", "assimetry");
        }

        private void MédiaXCurtoseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GenerateBivariateChart("mean", "curtose");
        }

        private void SelecionamentoXMédiaToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GenerateBivariateChart("selection", "mean");
        }

        private void SelecionamentoXAssimetriaToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GenerateBivariateChart("selection", "assimetry");
        }

        private void SelecionamentoXCurtoseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GenerateBivariateChart("selection", "curtose");
        }

        private void AssimetriaXMédiaToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GenerateBivariateChart("assimetry", "mean");
        }

        private void AssimetriaXSelecionamentoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GenerateBivariateChart("assimetry", "selection");
        }

        private void AssimetriaXCurtoseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GenerateBivariateChart("assimetry", "curtose");
        }

        private void CurtoseXMédiaToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GenerateBivariateChart("curtose", "mean");
        }

        private void CurtoseXSelecionamentoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GenerateBivariateChart("curtose", "selection");
        }

        private void CurtoseXAssimetriaToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GenerateBivariateChart("curtose", "assimetry");
        }

        private void MédiaXSelecionamentoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MédiaXSelecionamentoToolStripMenuItem1_Click(sender, e);
        }

        private void MédiaXAssimetriaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MédiaXAssimetriaToolStripMenuItem1_Click(sender, e);
        }

        private void MédiaXCurtoseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MédiaXCurtoseToolStripMenuItem1_Click(sender, e);
        }

        private void SelecionamentoXMédiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelecionamentoXMédiaToolStripMenuItem1_Click(sender, e);
        }

        private void SelecionamentoXAssimetriaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelecionamentoXAssimetriaToolStripMenuItem1_Click(sender, e);
        }

        private void SelecionamentoXCurtoseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelecionamentoXCurtoseToolStripMenuItem1_Click(sender, e);
        }

        private void AssimetriaXMédiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AssimetriaXMédiaToolStripMenuItem1_Click(sender, e);
        }

        private void AssimetriaXSelecionamentoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AssimetriaXSelecionamentoToolStripMenuItem1_Click(sender, e);
        }

        private void AssimetriaXCurtoseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AssimetriaXCurtoseToolStripMenuItem1_Click(sender, e);
        }

        private void CurtoseXMédiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurtoseXMédiaToolStripMenuItem1_Click(sender, e);
        }

        private void CurtoseXSelecionamentoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurtoseXSelecionamentoToolStripMenuItem1_Click(sender, e);
        }

        private void CurtoseXAssimetriaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurtoseXAssimetriaToolStripMenuItem1_Click(sender, e);
        }

        private void SobreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Abount form = new Abount();
            form.ShowDialog();
        }

        private void Button15_Click(object sender, EventArgs e)
        {
            DiagramaDeFolkGrosseirosToolStripMenuItem1_Click(sender, e);
        }

        private void Button16_Click(object sender, EventArgs e)
        {
            BivariadoToolStripMenuItem_Click(sender, e);
        }

        private void BivariadoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CorrelationOptions form = new CorrelationOptions();
            form.ParentForm = this;
            form.ShowDialog();
        }

        private void ConfiguraçõesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsForm form = new SettingsForm();
            form.ShowDialog();
        }

        private void DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            EditarToolStripMenuItem1_Click(sender, e);
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            //contextMenuStrip3.Show(button4, new System.Drawing.Point(0, button4.Height));
        }
    }
}
