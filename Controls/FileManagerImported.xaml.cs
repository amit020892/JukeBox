﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for FileManagerImported.xaml
    /// </summary>
    public partial class FileManagerImported : UserControl
    {
        public string TrackName { get; set; }
        public FileManagerImported(ImportFactory.ManagedFile managedFile)
        {
            InitializeComponent();
            TrackName = managedFile.FileName;
        }
    }
}
