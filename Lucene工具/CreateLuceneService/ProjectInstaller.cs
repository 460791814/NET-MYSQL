using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;


namespace CreateLuceneService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            this.Committed += new InstallEventHandler(ProjectInstaller_Committed);  
        }
        private void ProjectInstaller_Committed(object sender, InstallEventArgs e)         
        {              //参数为服务的名字              
            System.ServiceProcess.ServiceController controller = new System.ServiceProcess.ServiceController("LuceneTongBu");             
            controller.Start();       
        } 
    }
}
