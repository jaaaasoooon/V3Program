﻿#pragma checksum "..\..\UserCtrlRecord.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "418F450DA9F6B7A555A040F077237DC8C40A0F99D5AE1CC5C11BC8946738E6F9"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace BoqiangH5 {
    
    
    /// <summary>
    /// UserCtrlRecord
    /// </summary>
    public partial class UserCtrlRecord : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 13 "..\..\UserCtrlRecord.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid dataGridRecord;
        
        #line default
        #line hidden
        
        
        #line 97 "..\..\UserCtrlRecord.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnReadData;
        
        #line default
        #line hidden
        
        
        #line 98 "..\..\UserCtrlRecord.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnStopRead;
        
        #line default
        #line hidden
        
        
        #line 99 "..\..\UserCtrlRecord.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnClear;
        
        #line default
        #line hidden
        
        
        #line 100 "..\..\UserCtrlRecord.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnSave;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/BoqiangV3;component/userctrlrecord.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\UserCtrlRecord.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 7 "..\..\UserCtrlRecord.xaml"
            ((BoqiangH5.UserCtrlRecord)(target)).Loaded += new System.Windows.RoutedEventHandler(this.ucRecordInfo_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.dataGridRecord = ((System.Windows.Controls.DataGrid)(target));
            return;
            case 3:
            this.btnReadData = ((System.Windows.Controls.Button)(target));
            
            #line 97 "..\..\UserCtrlRecord.xaml"
            this.btnReadData.Click += new System.Windows.RoutedEventHandler(this.btnReadData_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.btnStopRead = ((System.Windows.Controls.Button)(target));
            
            #line 98 "..\..\UserCtrlRecord.xaml"
            this.btnStopRead.Click += new System.Windows.RoutedEventHandler(this.btnStopRead_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.btnClear = ((System.Windows.Controls.Button)(target));
            
            #line 99 "..\..\UserCtrlRecord.xaml"
            this.btnClear.Click += new System.Windows.RoutedEventHandler(this.btnClear_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.btnSave = ((System.Windows.Controls.Button)(target));
            
            #line 100 "..\..\UserCtrlRecord.xaml"
            this.btnSave.Click += new System.Windows.RoutedEventHandler(this.btnSave_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

