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
using ConscriptDesigner.WinForms;
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Game.Tiles;

namespace ConscriptDesigner.Anchorables {
	/// <summary>
	/// Interaction logic for SpriteBrowserControl.xaml
	/// </summary>
	public partial class TileDataBrowserControl : UserControl {

		private TileDataPreview tileDataPreview;

		private bool suppressEvents;

		public TileDataBrowserControl() {
			this.suppressEvents = true;
			InitializeComponent();

			this.tileDataPreview = new TileDataPreview();
			this.tileDataPreview.Refreshed += OnSpritePreviewRefreshed;
			this.tileDataPreview.HoverTileDataChanged += OnHoverTileDataChanged;
			this.host.Child = this.tileDataPreview;
			this.suppressEvents = false;

			OnHoverTileDataChanged();
		}

		private void OnHoverTileDataChanged(object sender = null, EventArgs e = null) {
			BaseTileData hoverTileData = tileDataPreview.HoverTileData;
			if (hoverTileData == null) {
				textBlockTileName.Text = "";
				statusTileInfo.Content = "";
			}
			else {
				textBlockTileName.Text = hoverTileData.Name;
				if (hoverTileData.Type == null) {
					if (hoverTileData is TileData)
						statusTileInfo.Content = "Type: Tile";
					else
						statusTileInfo.Content = "Type: EventTile";
				}
				else {
					statusTileInfo.Content = "Type: " + hoverTileData.Type.Name;
				}
			}
		}

		public void Dispose() {
			tileDataPreview.Dispose();
		}

		public void RefreshList() {
			tileDataPreview.RefreshList();
		}

		public void ClearList() {
			tileDataPreview.ClearList();
		}

		private void OnSpritePreviewRefreshed(object sender, EventArgs e) {
			/*suppressEvents = true;
			comboBoxSpriteSizes.Items.Clear();
			foreach (Point2I point in tileDataPreview.GetSpriteSizes()) {
				ComboBoxItem item = new ComboBoxItem();
				item.Content = point.X + " x " + point.Y;
				item.Tag = point;
				comboBoxSpriteSizes.Items.Add(item);
				if (point == tileDataPreview.SpriteSize) {
					comboBoxSpriteSizes.SelectedItem = item;
				}
			}
			suppressEvents = false;*/
		}

		private void OnSearchTextChanged(object sender, TextChangedEventArgs e) {
			if (suppressEvents) return;
			tileDataPreview.UpdateFilter(textBoxSearch.Text);
		}

		private void OnToggleAnimations(object sender, RoutedEventArgs e) {
			tileDataPreview.Animating = !tileDataPreview.Animating;
			buttonRestartAnimations.IsEnabled = tileDataPreview.Animating;
		}

		private void OnRestartAnimations(object sender, RoutedEventArgs e) {
			tileDataPreview.RestartAnimations();
		}
	}
}
