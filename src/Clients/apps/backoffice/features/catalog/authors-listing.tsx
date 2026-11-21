"use client";

import useAuthors from "@workspace/api-hooks/catalog/authors/useAuthors";
import useCreateAuthor from "@workspace/api-hooks/catalog/authors/useCreateAuthor";
import useDeleteAuthor from "@workspace/api-hooks/catalog/authors/useDeleteAuthor";
import useUpdateAuthor from "@workspace/api-hooks/catalog/authors/useUpdateAuthor";

import { SimpleCatalogListing } from "./simple-catalog-listing";

const breadcrumbs = [
  { label: "Admin", href: "/" },
  { label: "Authors", isActive: true },
];

const buildCreateRequest = (name: string) => ({ name });
const buildUpdateRequest = (id: string, name: string) => ({
  request: { id, name },
});

export function AuthorsListing() {
  return (
    <SimpleCatalogListing
      entityName="Author"
      pluralLabel="authors"
      pageTitle="Authors Management"
      pageDescription="Manage book authors"
      dialogTitle="Create Author"
      dialogDescription="Enter the author's name"
      dialogPlaceholder="Author name"
      tableTitle="All Authors"
      breadcrumbs={breadcrumbs}
      listQuery={useAuthors()}
      createMutation={useCreateAuthor()}
      updateMutation={useUpdateAuthor()}
      deleteMutation={useDeleteAuthor()}
      buildCreateRequest={buildCreateRequest}
      buildUpdateRequest={buildUpdateRequest}
    />
  );
}
