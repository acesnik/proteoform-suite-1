﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProteoformSuiteInternal;
using System.Windows.Forms;
using System.IO;
using System.Security;

namespace ProteoformSuite
{
    public partial class ProteoformFamilies : Form
    {
        OpenFileDialog fileOpener = new OpenFileDialog();
        FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
        bool got_cyto_temp_folder = false;
        double sleep_multiplier = 1;

        public ProteoformFamilies()
        {
            InitializeComponent();
        }

        private void ProteoformFamilies_Load(object sender, EventArgs e)
        { }

        private void initialize_settings()
        {
            this.tb_familyBuildFolder.Text = Lollipop.family_build_folder_path;
            this.nud_decimalRoundingLabels.Value = Convert.ToDecimal(Lollipop.deltaM_edge_display_rounding);
        }

        public void construct_families()
        {
            initialize_settings();
            if (Lollipop.proteoform_community.families.Count <= 0 && Lollipop.proteoform_community.has_e_proteoforms) run_the_gamut();
        }

        public DataGridView GetDGV()
        {
            return dgv_proteoform_families;
        }

        private void run_the_gamut()
        {
            this.Cursor = Cursors.WaitCursor;
            Lollipop.proteoform_community.construct_families();
            fill_proteoform_families();
            update_figures_of_merit();
            this.Cursor = Cursors.Default;
        }

        private void fill_proteoform_families()
        {
            DisplayUtility.FillDataGridView(dgv_proteoform_families, Lollipop.proteoform_community.families.OrderByDescending(f => f.relation_count).ToList());
            format_families_dgv();
        }

        private void update_figures_of_merit()
        {
            this.tb_TotalFamilies.Text = Lollipop.proteoform_community.families.Count(f => f.proteoforms.Count > 1).ToString();
            this.tb_IdentifiedFamilies.Text = Lollipop.proteoform_community.families.Count(f => f.theoretical_count > 0).ToString();
            this.tb_singleton_count.Text = Lollipop.proteoform_community.families.Count(f => f.proteoforms.Count == 1).ToString();
        }

        private void format_families_dgv()
        {
            //set column header
            //dgv_proteoform_families.Columns["family_id"].HeaderText = "Light Monoisotopic Mass";
            dgv_proteoform_families.Columns["lysine_count"].HeaderText = "Lysine Count";
            dgv_proteoform_families.Columns["experimental_count"].HeaderText = "Experimental Proteoforms";
            dgv_proteoform_families.Columns["theoretical_count"].HeaderText = "Theoretical Proteoforms";
            dgv_proteoform_families.Columns["relation_count"].HeaderText = "Relation Count";
            dgv_proteoform_families.Columns["accession_list"].HeaderText = "Theoretical Accessions";
            dgv_proteoform_families.Columns["name_list"].HeaderText = "Theoretical Names";
            dgv_proteoform_families.Columns["experimentals_list"].HeaderText = "Experimental Accessions";
            dgv_proteoform_families.Columns["agg_mass_list"].HeaderText = "Experimental Aggregated Masses";
            dgv_proteoform_families.Columns["relations"].Visible = false;
            dgv_proteoform_families.Columns["proteoforms"].Visible = false;
        }

        private void dgv_proteoform_families_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) display_family_members(e.RowIndex, e.ColumnIndex);
        }
        private void dgv_proteoform_families_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0) display_family_members(e.RowIndex, e.ColumnIndex);
        }
        private void display_family_members(int row_index, int column_index)
        {
            ProteoformFamily selected_family = (ProteoformFamily)this.dgv_proteoform_families.Rows[row_index].DataBoundItem;
            if (new List<string> { "theoretical_count", "accession_list","name_list" }.Contains(dgv_proteoform_families.Columns[column_index].Name))
            {
                if (selected_family.theoretical_count > 0) 
                {
                    DisplayUtility.FillDataGridView(dgv_proteoform_family_members, selected_family.theoretical_proteoforms);
                    DisplayUtility.FormatTheoreticalProteoformTable(dgv_proteoform_family_members);
                }
                else dgv_proteoform_family_members.Rows.Clear();
            }
            else if (new List<string> { "experimental_count", "experimentals_list", "agg_mass_list" }.Contains(dgv_proteoform_families.Columns[column_index].Name))
            {
                if (selected_family.experimental_count > 0)
                {
                    DisplayUtility.FillDataGridView(dgv_proteoform_family_members, selected_family.experimental_proteoforms);
                    DisplayUtility.FormatAggregatesTable(dgv_proteoform_family_members);
                }
                else dgv_proteoform_family_members.Rows.Clear();
            }
            else if (dgv_proteoform_families.Columns[column_index].Name == "relation_count")
            {
                if (selected_family.relation_count > 0)
                {
                    DisplayUtility.FillDataGridView(dgv_proteoform_family_members, selected_family.relations);
                    DisplayUtility.FormatRelationsGridView(dgv_proteoform_family_members, false, false);
                }
                else dgv_proteoform_family_members.Rows.Clear();
            }
        }

        private void btn_browseTempFolder_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.folderBrowser.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string temp_folder_path = folderBrowser.SelectedPath;
                tb_familyBuildFolder.Text = temp_folder_path; //triggers TextChanged method
            }
        }

        private void tb_tempFileFolderPath_TextChanged(object sender, EventArgs e)
        {
            string path = tb_familyBuildFolder.Text;
            Lollipop.family_build_folder_path = path;
            got_cyto_temp_folder = true;
            enable_buildAllFamilies_button();
            enable_buildSelectedFamilies_button();
        }

        private void enable_buildAllFamilies_button()
        {
            if (got_cyto_temp_folder) btn_buildAllFamilies.Enabled = true;
        }
        private void enable_buildSelectedFamilies_button()
        {
            if (got_cyto_temp_folder && dgv_proteoform_families.SelectedRows.Count > 0) btn_buildSelectedFamilies.Enabled = true;
        }

        private void btn_buildAllFamilies_Click(object sender, EventArgs e)
        {
            bool built = build_families(Lollipop.proteoform_community.families);
            if (!built) return;
            MessageBox.Show("Finished building all families.\n\nPlease load them into Cytoscape 3.0 or later using \"Tools\" -> \"Execute Command File\" and choosing the script_[TIMESTAMP].txt file in your specified directory.");
        }

        private void btn_buildSelectedFamilies_Click(object sender, EventArgs e)
        {
            //Check if there are any rows selected
            int selected_row_sum = 0;
            for (int i = 0; i < dgv_proteoform_families.SelectedCells.Count; i++) selected_row_sum += dgv_proteoform_families.SelectedCells[i].RowIndex;

            List<ProteoformFamily> families = new List<ProteoformFamily>();
            if (dgv_proteoform_families.SelectedRows.Count > 0)
                for (int i = 0; i < dgv_proteoform_families.SelectedRows.Count; i++)
                    families.Add((ProteoformFamily)dgv_proteoform_families.SelectedRows[i].DataBoundItem);
            else
                for (int i = 0; i < dgv_proteoform_families.SelectedCells.Count; i++)
                    if (dgv_proteoform_families.SelectedCells[i].RowIndex != 0)
                        families.Add((ProteoformFamily)dgv_proteoform_families.Rows[dgv_proteoform_families.SelectedCells[i].RowIndex].DataBoundItem);

            bool built = build_families(families);
            if (!built) return;

            string selected_family_string = "Finished building selected famil";
            if (families.Count() == 1) selected_family_string += "y :";
            else selected_family_string += "ies :";
            if (families.Count() > 3) selected_family_string = String.Join(", ", families.Select(f => f.family_id).ToList().Take(3)) + ". . .";
            else selected_family_string = String.Join(", ", families.Select(f => f.family_id));
            MessageBox.Show(selected_family_string + ".\n\nPlease load them into Cytoscape 3.0 or later using \"Tools\" -> \"Execute Command File\" and choosing the script_[TIMESTAMP].txt file in your specified directory.");
        }

        private bool build_families(List<ProteoformFamily> families)
        {
            //Check if valid folder
            if (Lollipop.family_build_folder_path == "" || !Directory.Exists(Lollipop.family_build_folder_path))
            {
                MessageBox.Show("Please choose a folder in which the families will be built, so you can load them into Cytoscape.");
                return false;
            }
            string time_stamp = SaveState.time_stamp();
            tb_recentTimeStamp.Text = time_stamp;
            CytoscapeScript c = new CytoscapeScript(families, time_stamp, sleep_multiplier);
            File.WriteAllText(c.edges_path, c.edge_table);
            File.WriteAllText(c.nodes_path, c.node_table);
            File.WriteAllText(c.script_path, c.script);
            c.write_styles();
            return true;
        }

        private void Families_update_Click(object sender, EventArgs e)
        {
            Lollipop.proteoform_community.families.Clear();
            run_the_gamut();
        }

        private void nud_decimalRoundingLabels_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.deltaM_edge_display_rounding = Convert.ToInt32(this.nud_decimalRoundingLabels.Value);
        }

        private void btn_sleepInfo_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Raise this value if Cytoscape isn't making proper proteoform family displays. This increases the sleep time between commands that construct the visualization.");
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            sleep_multiplier = Convert.ToDouble(this.nud_sleepFactor.Value);
        }
    }
}
