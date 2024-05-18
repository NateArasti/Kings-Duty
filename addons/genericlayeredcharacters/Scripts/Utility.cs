using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GenericLayeredCharacters
{
	public static class Utility
	{
		private const string k_CardboardLayerSuffix = "_cardboard";
		private const string k_OutlineLayerSuffix = "_cardboard_outline";
		
		private static readonly Dictionary<Layer, bool> s_LayersCardboardsFlag = new()
		{
			[Layer.Weapon] = true,
			[Layer.Hair] = true,
			[Layer.Eyes] = false,
			[Layer.Nose] = false,
			[Layer.Mouth] = false,
			[Layer.Head] = true,
			[Layer.Clothes] = false,
			[Layer.Body] = true,
			[Layer.Legs] = true,
		};
		
		public static readonly IReadOnlyDictionary<Layer, CharacterElement> LayerElements = new Dictionary<Layer, CharacterElement>()
		{
			[Layer.Weapon] = ResourceLoader.Load<CharacterElement>("res://addons/genericlayeredcharacters/Resources/Weapon.tres"),
			[Layer.Hair] = ResourceLoader.Load<CharacterElement>("res://addons/genericlayeredcharacters/Resources/Hair.tres"),
			[Layer.Eyes] = ResourceLoader.Load<CharacterElement>("res://addons/genericlayeredcharacters/Resources/Eyes.tres"),
			[Layer.Nose] = ResourceLoader.Load<CharacterElement>("res://addons/genericlayeredcharacters/Resources//Nose.tres"),
			[Layer.Mouth] = ResourceLoader.Load<CharacterElement>("res://addons/genericlayeredcharacters/Resources/Mouth.tres"),
			[Layer.Head] = ResourceLoader.Load<CharacterElement>("res://addons/genericlayeredcharacters/Resources/Head.tres"),
			[Layer.Clothes] = ResourceLoader.Load<CharacterElement>("res://addons/genericlayeredcharacters/Resources/Clothes.tres"),
			[Layer.Body] = ResourceLoader.Load<CharacterElement>("res://addons/genericlayeredcharacters/Resources/Body.tres"),
			[Layer.Legs] = ResourceLoader.Load<CharacterElement>("res://addons/genericlayeredcharacters/Resources/Legs.tres"),
		};
		
		private static readonly Dictionary<Layer, string> s_LayersMaterialPropertyNames = new()
		{
			[Layer.Weapon] = "weapon",
			[Layer.Hair] = "hair",
			[Layer.Eyes] = "eyes",
			[Layer.Nose] = "nose",
			[Layer.Mouth] = "mouth",
			[Layer.Head] = "head",
			[Layer.Clothes] = "clothes",
			[Layer.Body] = "body",
			[Layer.Legs] = "legs",
		};
		
		public static Texture2D EmptyTexture = ResourceLoader.Load<Texture2D>("res://addons/genericlayeredcharacters/EmptyTexture.png");
		
		public static IEnumerable<Layer> GetAllLayers()
		{
			foreach (Layer layer in Enum.GetValues(typeof(Layer)))
			{
				yield return layer;
			}
		}
		
		public static IReadOnlyList<Layer> GetAllDependentLayers(Layer layer)
		{
			return layer switch 
			{
				Layer.Weapon => new Layer[0],
				Layer.Hair => new Layer[0],
				Layer.Eyes => new Layer[0],
				Layer.Nose => new Layer[0],
				Layer.Mouth => new Layer[0],
				Layer.Head => new Layer[] { Layer.Hair, Layer.Eyes, Layer.Nose, Layer.Mouth },
				Layer.Clothes => new Layer[0],
				Layer.Body => new Layer[] { Layer.Head, Layer.Clothes, Layer.Legs, Layer.Weapon },
				Layer.Legs => new Layer[0],
				_ => new Layer[0],
			};
		}
		
		public static void SetElementVariationOnMaterial(ShaderMaterial material, ElementVariation elementVariation)
		{
			var layerProprtyName = s_LayersMaterialPropertyNames[elementVariation.Layer];
			if (elementVariation.Main == null)
			{
				GD.Print(elementVariation.Label);
			}
			material.SetShaderParameter(layerProprtyName, elementVariation.Main);
			if (s_LayersCardboardsFlag[elementVariation.Layer])
			{
				if (elementVariation is CardboardElementVariation cardboardElementVariation)
				{
					if (cardboardElementVariation.Cardboard == null)
					{
						GD.Print(elementVariation.Label);
					}
					if (cardboardElementVariation.Outline == null)
					{
						GD.Print(elementVariation.Label);
					}
					material.SetShaderParameter($"{layerProprtyName}{k_CardboardLayerSuffix}", cardboardElementVariation.Cardboard);
					material.SetShaderParameter($"{layerProprtyName}{k_OutlineLayerSuffix}", cardboardElementVariation.Outline);
				}
				else
				{
					material.SetShaderParameter($"{layerProprtyName}{k_CardboardLayerSuffix}", EmptyTexture);
					material.SetShaderParameter($"{layerProprtyName}{k_OutlineLayerSuffix}", EmptyTexture);
				}
			}
		}
		
		public static void ClearLayer(ShaderMaterial material, Layer layer)
		{
			var layerProprtyName = s_LayersMaterialPropertyNames[layer];
			material.SetShaderParameter(layerProprtyName, EmptyTexture);
			if (s_LayersCardboardsFlag[layer])
			{
				material.SetShaderParameter($"{layerProprtyName}{k_CardboardLayerSuffix}", EmptyTexture);
				material.SetShaderParameter($"{layerProprtyName}{k_OutlineLayerSuffix}", EmptyTexture);
			}
		}
		
		public static void ClearAllLayers(ShaderMaterial material)
		{
			foreach (var layer in GetAllLayers())
			{
				ClearLayer(material, layer);
			}
		}
		
		public static void FillRandomLayers(ShaderMaterial material, bool setWeapon = true)
		{
			ClearAllLayers(material);
			var chosenVariations = new Dictionary<Layer, ElementVariation>();
			
			// Setting random body layer
			var bodyElement = LayerElements[Layer.Body];
			chosenVariations[Layer.Body] = bodyElement.Variations[GD.RandRange(0, bodyElement.Variations.Length - 1)];
			SetElementVariationOnMaterial(material, chosenVariations[Layer.Body]);
			
			// Setting random available dependent body layers
			var possibleVariations = new Dictionary<Layer, List<ElementVariation>>();
			foreach (var layer in GetAllDependentLayers(Layer.Body))
			{
				possibleVariations[layer] = new();
			}
			foreach (var dependent in chosenVariations[Layer.Body].AvailableDependents)
			{
				possibleVariations[dependent.Layer].Add(dependent);
			}
			foreach (var layer in GetAllDependentLayers(chosenVariations[Layer.Body].Layer))
			{
				var layerVariations = possibleVariations[layer];
				chosenVariations[layer] = layerVariations[GD.RandRange(0, layerVariations.Count - 1)];
				SetElementVariationOnMaterial(material, chosenVariations[layer]);
			}
			
			// Setting random available dependent head layers
			possibleVariations.Clear();
			foreach (var layer in GetAllDependentLayers(Layer.Head))
			{
				possibleVariations[layer] = new();
			}
			foreach (var dependent in chosenVariations[Layer.Head].AvailableDependents)
			{
				possibleVariations[dependent.Layer].Add(dependent);
			}
			foreach (var layer in GetAllDependentLayers(chosenVariations[Layer.Head].Layer))
			{
				var layerVariations = possibleVariations[layer];
				chosenVariations[layer] = layerVariations[GD.RandRange(0, layerVariations.Count - 1)];
				SetElementVariationOnMaterial(material, chosenVariations[layer]);
			}
			
			if (setWeapon)
			{
				// Setting random weapon layer
				var weaponElement = LayerElements[Layer.Weapon];
				chosenVariations[Layer.Weapon] = weaponElement.Variations[GD.RandRange(0, weaponElement.Variations.Length - 1)];
				SetElementVariationOnMaterial(material, chosenVariations[Layer.Weapon]);
			}
		}
	}
}