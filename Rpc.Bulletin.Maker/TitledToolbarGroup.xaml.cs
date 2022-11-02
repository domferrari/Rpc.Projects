using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rpc.Bulletin.Maker
{
	[ContentProperty("AdditionalContent")]
	public partial class TitledToolbarGroup : UserControl
	{
		public TitledToolbarGroup()
		{
			InitializeComponent();
		}

		public string Title
		{
			get => _txtTitle.Text;
			set => _txtTitle.Text = value;
		}

		public object AdditionalContent
		{
			get { return GetValue(AdditionalContentProperty); }
			set { SetValue(AdditionalContentProperty, value); }
		}

		public static readonly DependencyProperty AdditionalContentProperty =
			DependencyProperty.Register(
				"AdditionalContent",
				typeof(object), 
				typeof(TitledToolbarGroup),
				new PropertyMetadata(null));
	}
}
