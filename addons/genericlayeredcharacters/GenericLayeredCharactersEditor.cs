#if TOOLS
using System;
using Godot;

namespace GenericLayeredCharacters
{
	[Tool]
	public partial class GenericLayeredCharactersEditor : EditorPlugin
	{
		private const string k_PluginName = "Generic Characters";
		
		private Control m_PluginGUI;
		
		private Button m_ClearButton;
		private Button m_RandomButton;
		private Button m_SelectAll;
		private ItemList m_AllLayersList;
		private ItemList m_LayerVariationsList;
		private ItemList m_DependentLayersList;
		private Control m_AvaialableVariationsListPivot;
		private CheckButton m_AvaialableVariationToggleReference;
		
		private ShaderMaterial m_CharacterPreviewMaterial;
		
		private CharacterElement m_SelectedCharacterElement;
		private ElementVariation m_SelectedElementVariation;
		private CharacterElement m_SelectedDependantCharacterElement;
		
		private readonly PackedScene m_PluginScene = ResourceLoader.Load<PackedScene>("res://addons/genericlayeredcharacters/Scenes/PluginUI.tscn");

		#region Plugin
		
		public override void _EnterTree()
		{
			m_PluginGUI = m_PluginScene.Instantiate<Control>();
			EditorInterface.Singleton.GetEditorMainScreen().AddChild(m_PluginGUI);
			_MakeVisible(false);
			
			AddCustomResources();
			Init();
		}
		
		private void AddCustomResources()
		{
			AddCustomType(nameof(CharacterElement), nameof(Resource), GD.Load<Script>("res://addons/genericlayeredcharacters/Scripts/Base/CharacterElement.cs"), null);
			AddCustomType(nameof(ElementVariation), nameof(Resource), GD.Load<Script>("res://addons/genericlayeredcharacters/Scripts/Base/ElementVariation.cs"), null);
		}
		
		private void RemoveCustomResources()
		{
			RemoveCustomType(nameof(CharacterElement));
			RemoveCustomType(nameof(ElementVariation));
		}

		public override void _ExitTree()
		{
			if (m_PluginGUI != null)
			{
				m_PluginGUI.QueueFree();
			}
			
			RemoveCustomResources();
			DiscardDependencies();
		}

		public override bool _HasMainScreen() => true;

		public override void _MakeVisible(bool visible)
		{
			if (m_PluginGUI != null)
			{
				m_PluginGUI.Visible = visible;
			}
		}

		public override string _GetPluginName()
		{
			return k_PluginName;
		}
		
		#endregion
		
		#region UI
		
		private void Init()
		{
			GatherReferences();
			
			m_AllLayersList.Clear();
			foreach (var layer in Utility.GetAllLayers())
			{
				m_AllLayersList.AddItem(layer.ToString());
			}
			m_RandomButton.Pressed += SetRandomCharacter;
			
			m_ClearButton.Pressed += ClearPreview;
			m_AllLayersList.ItemSelected += SelectElement;
			m_SelectAll.Pressed += SelectAllVariations;
			m_LayerVariationsList.ItemSelected += SelectElementVariation;
			m_DependentLayersList.ItemSelected += SelectDependentLayer;
			
			ClearPreview();
		}

		private void DiscardDependencies()
		{
			m_RandomButton.Pressed -= SetRandomCharacter;
			m_ClearButton.Pressed -= ClearPreview;
			m_AllLayersList.ItemSelected -= SelectElement;
			m_LayerVariationsList.ItemSelected -= SelectElementVariation;
			m_DependentLayersList.ItemSelected -= SelectDependentLayer;
			ClearPreview();
		}

		private void GatherReferences()
		{
			m_ClearButton = m_PluginGUI.GetNode<Button>("Buttons/Clear");
			m_RandomButton = m_PluginGUI.GetNode<Button>("Buttons/Random");
			m_SelectAll = m_PluginGUI.GetNode<Button>("Buttons/SelectAl");
			m_AllLayersList = m_PluginGUI.GetNode<ItemList>("Left/AllElementsList");
			m_LayerVariationsList = m_PluginGUI.GetNode<ItemList>("Left/VariationsList");
			m_DependentLayersList = m_PluginGUI.GetNode<ItemList>("Right/DependantBodyElementsList");
			m_AvaialableVariationsListPivot = m_PluginGUI.GetNode<Control>("Right/SelectedVariations/MarginContainer/ScrollContainer/VBoxContainer");
			m_AvaialableVariationToggleReference = m_PluginGUI.GetNode<CheckButton>("Right/SelectedVariations/MarginContainer/ScrollContainer/VBoxContainer/CheckButton");
			
			m_CharacterPreviewMaterial = m_PluginGUI.GetNode<TextureRect>("CharacterPreview").Material as ShaderMaterial;
		}

		private void SelectAllVariations()
		{
			if (m_SelectedElementVariation == null) return;
			
			foreach (var layer in Utility.GetAllDependentLayers(m_SelectedCharacterElement.Layer))
			{
				var dependentLayer = Utility.LayerElements[layer];
				foreach (var elementVariation in dependentLayer.Variations)
				{
					if (m_SelectedElementVariation.AvailableDependents.Contains(elementVariation)) continue;
					m_SelectedElementVariation.AvailableDependents.Add(elementVariation);
				}
			}
			ResourceSaver.Save(m_SelectedElementVariation);
			
			// regenerating elements if needed
			if (m_SelectedDependantCharacterElement != null)
			{
				FillAvailableVariationList();
			}
		}

		private void SetRandomCharacter()
		{
			Utility.FillRandomLayers(m_CharacterPreviewMaterial);
		}

		private void ClearPreview()
		{
			Utility.ClearAllLayers(m_CharacterPreviewMaterial);
			
			m_SelectedCharacterElement = null;
			m_AllLayersList.DeselectAll();
			
			m_LayerVariationsList.DeselectAll();
			m_LayerVariationsList.Clear();
			m_DependentLayersList.DeselectAll();
			m_DependentLayersList.Clear();
			
			m_SelectedElementVariation = null;
			m_SelectAll.Visible = false;
			
			ClearAvailableVariationList();
		}

		private void SelectElement(long index)
		{
			var layer = (Layer)index;
			m_SelectedCharacterElement = Utility.LayerElements[layer];
			
			m_SelectedElementVariation = null;
			m_SelectAll.Visible = false;
			m_LayerVariationsList.DeselectAll();
			m_LayerVariationsList.Clear();
			foreach (var elementVariation in m_SelectedCharacterElement.Variations)
			{
				m_LayerVariationsList.AddItem(elementVariation.Label, elementVariation.Main);
			}
			
			m_SelectedDependantCharacterElement = null;
			m_DependentLayersList.DeselectAll();
			m_DependentLayersList.Clear();
			foreach (var dependentLayer in Utility.GetAllDependentLayers(layer))
			{
				m_DependentLayersList.AddItem(dependentLayer.ToString());
			}
			
			ClearAvailableVariationList();
		}

		private void SelectElementVariation(long index)
		{
			m_SelectedElementVariation = m_SelectedCharacterElement.Variations[index];
			Utility.SetElementVariationOnMaterial(m_CharacterPreviewMaterial, m_SelectedElementVariation);
			
			m_SelectAll.Visible = true;
			
			if (m_SelectedDependantCharacterElement != null)
			{
				FillAvailableVariationList();
			}
		}

		private void SelectDependentLayer(long index)
		{
			var layer = Utility.GetAllDependentLayers(m_SelectedCharacterElement.Layer)[(int)index];
			m_SelectedDependantCharacterElement = Utility.LayerElements[layer];
			
			if (m_SelectedElementVariation != null)
			{
				FillAvailableVariationList();
			}
		}
		
		private void FillAvailableVariationList()
		{
			ClearAvailableVariationList();
			
			foreach (var elementVariation in m_SelectedDependantCharacterElement.Variations)
			{
				var variationToggle = m_AvaialableVariationToggleReference.Duplicate() as CheckButton;
				variationToggle.Text = elementVariation.Label;
				variationToggle.Icon = elementVariation.Main;
				variationToggle.ButtonPressed = m_SelectedElementVariation.AvailableDependents.Contains(elementVariation);
				var chosenVariation = elementVariation;
				variationToggle.Toggled += (state) => MarkVariationAvailable(state, chosenVariation);
				variationToggle.Visible = true;
				m_AvaialableVariationsListPivot.AddChild(variationToggle);
			}
		}

		private void MarkVariationAvailable(bool state, ElementVariation elementVariation)
		{
			if (state)
			{
				if (m_SelectedElementVariation.AvailableDependents.Contains(elementVariation)) return;
				m_SelectedElementVariation.AvailableDependents.Add(elementVariation);
				Utility.SetElementVariationOnMaterial(m_CharacterPreviewMaterial, elementVariation);
			}
			else
			{
				m_SelectedElementVariation.AvailableDependents.Remove(elementVariation);
				Utility.ClearLayer(m_CharacterPreviewMaterial, elementVariation.Layer);
			}
			ResourceSaver.Save(m_SelectedElementVariation);
		}

		private void ClearAvailableVariationList()
		{
			foreach (var child in m_AvaialableVariationsListPivot.GetChildren()[1..])
			{
				child.QueueFree();
			}
		}
		
		#endregion
	}
}
#endif
