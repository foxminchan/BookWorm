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

  const { mutateAsync: createMutateAsync, isPending: isCreatePending } =
    createMutation;
  const { mutateAsync: updateMutateAsync, isPending: isUpdatePending } =
    updateMutation;
  const { mutateAsync: deleteMutateAsync, isPending: isDeletePending } =
    deleteMutation;

  const handleCreate = useCallback(
    async (name: string) => {
      try {
        await createMutateAsync(buildCreateRequest(name));
        setIsDialogOpen(false);
        toast.success(`${entityName} has been created`);
      } catch {
        toast.error(`Failed to create ${entityName.toLowerCase()}`);
      }
    },
    [createMutateAsync, buildCreateRequest, entityName],
  );

  const handleUpdate = useCallback(
    async (id: string, name: string) => {
      try {
        await updateMutateAsync(buildUpdateRequest(id, name));
        toast.success(`${entityName} has been updated`);
      } catch {
        toast.error(`Failed to update ${entityName.toLowerCase()}`);
      }
    },
    [updateMutateAsync, buildUpdateRequest, entityName],
  );

  const handleDelete = useCallback(
    async (id: string) => {
      try {
        await deleteMutateAsync(id);
        toast.success(`${entityName} has been deleted`);
      } catch {
        toast.error(`Failed to delete ${entityName.toLowerCase()}`);
      }
    },
    [deleteMutateAsync, entityName],
  );

  const isSubmitting = useMemo(
    () => isCreatePending || isUpdatePending || isDeletePending,
    [isCreatePending, isUpdatePending, isDeletePending],
  );

  return {
    items: listQuery.data ?? [],
    isLoading: listQuery.isLoading,
    isDialogOpen,
    setIsDialogOpen,
    isSubmitting,
    isCreatePending,
    handleCreate,
    handleUpdate,
    handleDelete,
  } as const;
}
