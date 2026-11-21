"use client";

import useCategories from "@workspace/api-hooks/catalog/categories/useCategories";
import useCreateCategory from "@workspace/api-hooks/catalog/categories/useCreateCategory";
import useDeleteCategory from "@workspace/api-hooks/catalog/categories/useDeleteCategory";
import useUpdateCategory from "@workspace/api-hooks/catalog/categories/useUpdateCategory";

import { SimpleCatalogListing } from "./simple-catalog-listing";

const breadcrumbs = [
  { label: "Admin", href: "/" },
  { label: "Categories", isActive: true },
];

const buildCreateRequest = (name: string) => ({ name });
const buildUpdateRequest = (id: string, name: string) => ({
  request: { id, name },
});

export function CategoriesListing() {
  return (
    <SimpleCatalogListing
      entityName="Category"
      pluralLabel="categories"
      pageTitle="Categories Management"
      pageDescription="Manage book categories"
      dialogTitle="Create Category"
      dialogDescription="Enter the category name"
      dialogPlaceholder="Category name"
      tableTitle="All Categories"
      breadcrumbs={breadcrumbs}
      listQuery={useCategories()}
      createMutation={useCreateCategory()}
      updateMutation={useUpdateCategory()}
      deleteMutation={useDeleteCategory()}
      buildCreateRequest={buildCreateRequest}
      buildUpdateRequest={buildUpdateRequest}
    />
  );
}
