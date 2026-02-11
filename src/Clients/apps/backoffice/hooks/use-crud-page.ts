"use client";

import { useCallback, useMemo, useState } from "react";

import type { UseMutationResult, UseQueryResult } from "@tanstack/react-query";
import { toast } from "sonner";

type CrudPageOptions<
  TItem,
  TCreateResult,
  TUpdateResult,
  TCreateRequest,
  TUpdateRequest,
> = Readonly<{
  entityName: string;
  listQuery: UseQueryResult<TItem[]>;
  createMutation: UseMutationResult<TCreateResult, Error, TCreateRequest>;
  updateMutation: UseMutationResult<TUpdateResult, Error, TUpdateRequest>;
  deleteMutation: UseMutationResult<void, Error, string>;
  buildCreateRequest: (name: string) => TCreateRequest;
  buildUpdateRequest: (id: string, name: string) => TUpdateRequest;
}>;

export function useCrudPage<
  TItem,
  TCreateResult,
  TUpdateResult,
  TCreateRequest,
  TUpdateRequest,
>({
  entityName,
  listQuery,
  createMutation,
  updateMutation,
  deleteMutation,
  buildCreateRequest,
  buildUpdateRequest,
}: CrudPageOptions<
  TItem,
  TCreateResult,
  TUpdateResult,
  TCreateRequest,
  TUpdateRequest
>) {
  const [isDialogOpen, setIsDialogOpen] = useState(false);

  const handleCreate = useCallback(
    async (name: string) => {
      await createMutation.mutateAsync(buildCreateRequest(name));
      setIsDialogOpen(false);
      toast.success(`${entityName} has been created`);
    },
    [createMutation, buildCreateRequest, entityName],
  );

  const handleUpdate = useCallback(
    async (id: string, name: string) => {
      await updateMutation.mutateAsync(buildUpdateRequest(id, name));
      toast.success(`${entityName} has been updated`);
    },
    [updateMutation, buildUpdateRequest, entityName],
  );

  const handleDelete = useCallback(
    async (id: string) => {
      await deleteMutation.mutateAsync(id);
      toast.success(`${entityName} has been deleted`);
    },
    [deleteMutation, entityName],
  );

  const isSubmitting = useMemo(
    () =>
      createMutation.isPending ||
      updateMutation.isPending ||
      deleteMutation.isPending,
    [
      createMutation.isPending,
      updateMutation.isPending,
      deleteMutation.isPending,
    ],
  );

  return {
    items: listQuery.data ?? [],
    isLoading: listQuery.isLoading,
    isDialogOpen,
    setIsDialogOpen,
    isSubmitting,
    isCreatePending: createMutation.isPending,
    handleCreate,
    handleUpdate,
    handleDelete,
  } as const;
}
