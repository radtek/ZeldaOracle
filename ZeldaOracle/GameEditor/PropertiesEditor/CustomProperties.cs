﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using ZeldaOracle.Common.Graphics;
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Common.Content;
using ZeldaEditor.Control;
using ZeldaOracle.Game.Items.Rewards;
using ZeldaOracle.Common.Scripting;


namespace ZeldaEditor.PropertiesEditor {
	

	[TypeConverter(typeof(PropertiesContainer.CustomObjectConverter))]
	public class PropertiesContainer {
		
		private PropertyGridControl	propertyGridControl;
		private Properties			properties;
		private List<Property>		propertyList;

		
		//-----------------------------------------------------------------------------
		// Constructor
		//-----------------------------------------------------------------------------

		public PropertiesContainer(PropertyGridControl propertyGridControl) {
			this.properties				= null;
			this.propertyList			= new List<Property>();
			this.propertyGridControl	= propertyGridControl;
		}
		
		
		//-----------------------------------------------------------------------------
		// Mutators
		//-----------------------------------------------------------------------------

		public void Clear() {
			properties = null;
			propertyList.Clear();
		}

		public void AddProperties(Properties properties) {
			foreach (Property property in  properties.GetAllProperties()) {
				PropertyDocumentation doc = property.GetRootDocumentation();
				if (doc == null || !doc.IsHidden)
					propertyList.Add(property);
			}
		}

		public void Set(Properties properties) {
			Clear();
			this.properties = properties;
			AddProperties(properties);
		}


		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------

		[Browsable(false)]
		public Properties Properties {
			get { return properties; }
		}

		[Browsable(false)]
		public List<Property> PropertyList {
			get { return propertyList; }
		}

		[Browsable(false)]
		public PropertyGridControl PropertyGridControl {
			get { return propertyGridControl; }
		}


		//-----------------------------------------------------------------------------
		// Internal Classes
		//-----------------------------------------------------------------------------

		private class CustomObjectConverter : ExpandableObjectConverter {
			public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes) {
				PropertiesContainer obj = (value as PropertiesContainer);
				if (obj == null)
					return new PropertyDescriptorCollection(new PropertyDescriptor[] {});

				List<Property> propertyList	= obj.PropertyList;
				PropertyDescriptor[] props	= new PropertyDescriptor[propertyList.Count];

				// Create the list of property descriptors.
				for (int i = 0; i < propertyList.Count; i++) {
					Property property	= propertyList[i];
					string name			= property.Name;
					UITypeEditor editor = null;
					PropertyDocumentation documentation = property.GetDocumentation();
					
					// Find the editor.
					if (documentation != null)
						editor = obj.PropertyGridControl.GetUITypeEditor(documentation.EditorType);

					// Create the property descriptor.
					props[i] = new CustomPropertyDescriptor(
						documentation, editor, property.Name, obj.Properties);
				}

				return new PropertyDescriptorCollection(props);
			}
		}

	}

}
