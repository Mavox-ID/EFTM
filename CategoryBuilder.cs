using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


public class CategoryBuilder
{
    public static void Create(string name, string description, Sprite icon)
    {
        CatalogBehaviour manager = UnityEngine.Object.FindObjectOfType<CatalogBehaviour>();
        if (manager.Catalog.Categories.FirstOrDefault((Category c) => c.name == name) == null)
        {
            Category category = ScriptableObject.CreateInstance<Category>();
            category.name = name;
            category.Description = description;
            category.Icon = icon;
            Category[] NewCategories = new Category[manager.Catalog.Categories.Length + 1];
            Category[] categories = manager.Catalog.Categories;
            for (int i = 0; i < categories.Length; i++)
            {
                NewCategories[i] = categories[i];
            }
            NewCategories[NewCategories.Length - 1] = category;
            manager.Catalog.Categories = NewCategories;
        }
    }

    // Created by 144^2
    // By head :>
}
