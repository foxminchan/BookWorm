using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWorm.Scheduler.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateScheduler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cron_ticker_occurrences_cron_tickers_cron_ticker_id",
                schema: "ticker",
                table: "CronTickerOccurrences"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_outbox_message_inbox_state_inbox_message_id_inbox_consumer_",
                table: "outbox_message"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_outbox_message_outbox_state_outbox_id",
                table: "outbox_message"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_time_tickers_time_tickers_batch_parent",
                schema: "ticker",
                table: "TimeTickers"
            );

            migrationBuilder.DropPrimaryKey(
                name: "pk_time_tickers",
                schema: "ticker",
                table: "TimeTickers"
            );

            migrationBuilder.DropIndex(
                name: "IX_TimeTicker_Status_ExecutionTime",
                schema: "ticker",
                table: "TimeTickers"
            );

            migrationBuilder.DropPrimaryKey(
                name: "pk_cron_tickers",
                schema: "ticker",
                table: "CronTickers"
            );

            migrationBuilder.DropPrimaryKey(
                name: "pk_cron_ticker_occurrences",
                schema: "ticker",
                table: "CronTickerOccurrences"
            );

            migrationBuilder.DropPrimaryKey(name: "pk_outbox_state", table: "outbox_state");

            migrationBuilder.DropPrimaryKey(name: "pk_outbox_message", table: "outbox_message");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_inbox_state_message_id_consumer_id",
                table: "inbox_state"
            );

            migrationBuilder.DropPrimaryKey(name: "pk_inbox_state", table: "inbox_state");

            migrationBuilder.RenameTable(name: "outbox_state", newName: "OutboxState");

            migrationBuilder.RenameTable(name: "outbox_message", newName: "OutboxMessage");

            migrationBuilder.RenameTable(name: "inbox_state", newName: "InboxState");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "ticker",
                table: "TimeTickers",
                newName: "Status"
            );

            migrationBuilder.RenameColumn(
                name: "retries",
                schema: "ticker",
                table: "TimeTickers",
                newName: "Retries"
            );

            migrationBuilder.RenameColumn(
                name: "request",
                schema: "ticker",
                table: "TimeTickers",
                newName: "Request"
            );

            migrationBuilder.RenameColumn(
                name: "function",
                schema: "ticker",
                table: "TimeTickers",
                newName: "Function"
            );

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "ticker",
                table: "TimeTickers",
                newName: "Description"
            );

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "ticker",
                table: "TimeTickers",
                newName: "Id"
            );

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "ticker",
                table: "TimeTickers",
                newName: "UpdatedAt"
            );

            migrationBuilder.RenameColumn(
                name: "retry_intervals",
                schema: "ticker",
                table: "TimeTickers",
                newName: "RetryIntervals"
            );

            migrationBuilder.RenameColumn(
                name: "retry_count",
                schema: "ticker",
                table: "TimeTickers",
                newName: "RetryCount"
            );

            migrationBuilder.RenameColumn(
                name: "locked_at",
                schema: "ticker",
                table: "TimeTickers",
                newName: "LockedAt"
            );

            migrationBuilder.RenameColumn(
                name: "lock_holder",
                schema: "ticker",
                table: "TimeTickers",
                newName: "LockHolder"
            );

            migrationBuilder.RenameColumn(
                name: "init_identifier",
                schema: "ticker",
                table: "TimeTickers",
                newName: "InitIdentifier"
            );

            migrationBuilder.RenameColumn(
                name: "execution_time",
                schema: "ticker",
                table: "TimeTickers",
                newName: "ExecutionTime"
            );

            migrationBuilder.RenameColumn(
                name: "executed_at",
                schema: "ticker",
                table: "TimeTickers",
                newName: "ExecutedAt"
            );

            migrationBuilder.RenameColumn(
                name: "elapsed_time",
                schema: "ticker",
                table: "TimeTickers",
                newName: "ElapsedTime"
            );

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "ticker",
                table: "TimeTickers",
                newName: "CreatedAt"
            );

            migrationBuilder.RenameColumn(
                name: "exception",
                schema: "ticker",
                table: "TimeTickers",
                newName: "SkippedReason"
            );

            migrationBuilder.RenameColumn(
                name: "batch_run_condition",
                schema: "ticker",
                table: "TimeTickers",
                newName: "RunCondition"
            );

            migrationBuilder.RenameColumn(
                name: "batch_parent",
                schema: "ticker",
                table: "TimeTickers",
                newName: "ParentId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_time_tickers_batch_parent",
                schema: "ticker",
                table: "TimeTickers",
                newName: "IX_TimeTickers_ParentId"
            );

            migrationBuilder.RenameColumn(
                name: "retries",
                schema: "ticker",
                table: "CronTickers",
                newName: "Retries"
            );

            migrationBuilder.RenameColumn(
                name: "request",
                schema: "ticker",
                table: "CronTickers",
                newName: "Request"
            );

            migrationBuilder.RenameColumn(
                name: "function",
                schema: "ticker",
                table: "CronTickers",
                newName: "Function"
            );

            migrationBuilder.RenameColumn(
                name: "expression",
                schema: "ticker",
                table: "CronTickers",
                newName: "Expression"
            );

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "ticker",
                table: "CronTickers",
                newName: "Description"
            );

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "ticker",
                table: "CronTickers",
                newName: "Id"
            );

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "ticker",
                table: "CronTickers",
                newName: "UpdatedAt"
            );

            migrationBuilder.RenameColumn(
                name: "retry_intervals",
                schema: "ticker",
                table: "CronTickers",
                newName: "RetryIntervals"
            );

            migrationBuilder.RenameColumn(
                name: "init_identifier",
                schema: "ticker",
                table: "CronTickers",
                newName: "InitIdentifier"
            );

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "ticker",
                table: "CronTickers",
                newName: "CreatedAt"
            );

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "ticker",
                table: "CronTickerOccurrences",
                newName: "Status"
            );

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "ticker",
                table: "CronTickerOccurrences",
                newName: "Id"
            );

            migrationBuilder.RenameColumn(
                name: "retry_count",
                schema: "ticker",
                table: "CronTickerOccurrences",
                newName: "RetryCount"
            );

            migrationBuilder.RenameColumn(
                name: "locked_at",
                schema: "ticker",
                table: "CronTickerOccurrences",
                newName: "LockedAt"
            );

            migrationBuilder.RenameColumn(
                name: "lock_holder",
                schema: "ticker",
                table: "CronTickerOccurrences",
                newName: "LockHolder"
            );

            migrationBuilder.RenameColumn(
                name: "execution_time",
                schema: "ticker",
                table: "CronTickerOccurrences",
                newName: "ExecutionTime"
            );

            migrationBuilder.RenameColumn(
                name: "executed_at",
                schema: "ticker",
                table: "CronTickerOccurrences",
                newName: "ExecutedAt"
            );

            migrationBuilder.RenameColumn(
                name: "elapsed_time",
                schema: "ticker",
                table: "CronTickerOccurrences",
                newName: "ElapsedTime"
            );

            migrationBuilder.RenameColumn(
                name: "cron_ticker_id",
                schema: "ticker",
                table: "CronTickerOccurrences",
                newName: "CronTickerId"
            );

            migrationBuilder.RenameColumn(
                name: "exception",
                schema: "ticker",
                table: "CronTickerOccurrences",
                newName: "SkippedReason"
            );

            migrationBuilder.RenameColumn(
                name: "delivered",
                table: "OutboxState",
                newName: "Delivered"
            );

            migrationBuilder.RenameColumn(
                name: "created",
                table: "OutboxState",
                newName: "Created"
            );

            migrationBuilder.RenameColumn(
                name: "row_version",
                table: "OutboxState",
                newName: "RowVersion"
            );

            migrationBuilder.RenameColumn(name: "lock_id", table: "OutboxState", newName: "LockId");

            migrationBuilder.RenameColumn(
                name: "last_sequence_number",
                table: "OutboxState",
                newName: "LastSequenceNumber"
            );

            migrationBuilder.RenameColumn(
                name: "outbox_id",
                table: "OutboxState",
                newName: "OutboxId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_outbox_state_created",
                table: "OutboxState",
                newName: "IX_OutboxState_Created"
            );

            migrationBuilder.RenameColumn(
                name: "properties",
                table: "OutboxMessage",
                newName: "Properties"
            );

            migrationBuilder.RenameColumn(
                name: "headers",
                table: "OutboxMessage",
                newName: "Headers"
            );

            migrationBuilder.RenameColumn(name: "body", table: "OutboxMessage", newName: "Body");

            migrationBuilder.RenameColumn(
                name: "source_address",
                table: "OutboxMessage",
                newName: "SourceAddress"
            );

            migrationBuilder.RenameColumn(
                name: "sent_time",
                table: "OutboxMessage",
                newName: "SentTime"
            );

            migrationBuilder.RenameColumn(
                name: "response_address",
                table: "OutboxMessage",
                newName: "ResponseAddress"
            );

            migrationBuilder.RenameColumn(
                name: "request_id",
                table: "OutboxMessage",
                newName: "RequestId"
            );

            migrationBuilder.RenameColumn(
                name: "outbox_id",
                table: "OutboxMessage",
                newName: "OutboxId"
            );

            migrationBuilder.RenameColumn(
                name: "message_type",
                table: "OutboxMessage",
                newName: "MessageType"
            );

            migrationBuilder.RenameColumn(
                name: "message_id",
                table: "OutboxMessage",
                newName: "MessageId"
            );

            migrationBuilder.RenameColumn(
                name: "initiator_id",
                table: "OutboxMessage",
                newName: "InitiatorId"
            );

            migrationBuilder.RenameColumn(
                name: "inbox_message_id",
                table: "OutboxMessage",
                newName: "InboxMessageId"
            );

            migrationBuilder.RenameColumn(
                name: "inbox_consumer_id",
                table: "OutboxMessage",
                newName: "InboxConsumerId"
            );

            migrationBuilder.RenameColumn(
                name: "fault_address",
                table: "OutboxMessage",
                newName: "FaultAddress"
            );

            migrationBuilder.RenameColumn(
                name: "expiration_time",
                table: "OutboxMessage",
                newName: "ExpirationTime"
            );

            migrationBuilder.RenameColumn(
                name: "enqueue_time",
                table: "OutboxMessage",
                newName: "EnqueueTime"
            );

            migrationBuilder.RenameColumn(
                name: "destination_address",
                table: "OutboxMessage",
                newName: "DestinationAddress"
            );

            migrationBuilder.RenameColumn(
                name: "correlation_id",
                table: "OutboxMessage",
                newName: "CorrelationId"
            );

            migrationBuilder.RenameColumn(
                name: "conversation_id",
                table: "OutboxMessage",
                newName: "ConversationId"
            );

            migrationBuilder.RenameColumn(
                name: "content_type",
                table: "OutboxMessage",
                newName: "ContentType"
            );

            migrationBuilder.RenameColumn(
                name: "sequence_number",
                table: "OutboxMessage",
                newName: "SequenceNumber"
            );

            migrationBuilder.RenameIndex(
                name: "ix_outbox_message_outbox_id_sequence_number",
                table: "OutboxMessage",
                newName: "IX_OutboxMessage_OutboxId_SequenceNumber"
            );

            migrationBuilder.RenameIndex(
                name: "ix_outbox_message_inbox_message_id_inbox_consumer_id_sequence_",
                table: "OutboxMessage",
                newName: "IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber"
            );

            migrationBuilder.RenameIndex(
                name: "ix_outbox_message_expiration_time",
                table: "OutboxMessage",
                newName: "IX_OutboxMessage_ExpirationTime"
            );

            migrationBuilder.RenameIndex(
                name: "ix_outbox_message_enqueue_time",
                table: "OutboxMessage",
                newName: "IX_OutboxMessage_EnqueueTime"
            );

            migrationBuilder.RenameColumn(
                name: "received",
                table: "InboxState",
                newName: "Received"
            );

            migrationBuilder.RenameColumn(
                name: "delivered",
                table: "InboxState",
                newName: "Delivered"
            );

            migrationBuilder.RenameColumn(
                name: "consumed",
                table: "InboxState",
                newName: "Consumed"
            );

            migrationBuilder.RenameColumn(name: "id", table: "InboxState", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "row_version",
                table: "InboxState",
                newName: "RowVersion"
            );

            migrationBuilder.RenameColumn(
                name: "receive_count",
                table: "InboxState",
                newName: "ReceiveCount"
            );

            migrationBuilder.RenameColumn(
                name: "message_id",
                table: "InboxState",
                newName: "MessageId"
            );

            migrationBuilder.RenameColumn(name: "lock_id", table: "InboxState", newName: "LockId");

            migrationBuilder.RenameColumn(
                name: "last_sequence_number",
                table: "InboxState",
                newName: "LastSequenceNumber"
            );

            migrationBuilder.RenameColumn(
                name: "expiration_time",
                table: "InboxState",
                newName: "ExpirationTime"
            );

            migrationBuilder.RenameColumn(
                name: "consumer_id",
                table: "InboxState",
                newName: "ConsumerId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_inbox_state_delivered",
                table: "InboxState",
                newName: "IX_InboxState_Delivered"
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExecutionTime",
                schema: "ticker",
                table: "TimeTickers",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone"
            );

            migrationBuilder.AddColumn<string>(
                name: "ExceptionMessage",
                schema: "ticker",
                table: "TimeTickers",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "ticker",
                table: "CronTickerOccurrences",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
            );

            migrationBuilder.AddColumn<string>(
                name: "ExceptionMessage",
                schema: "ticker",
                table: "CronTickerOccurrences",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "ticker",
                table: "CronTickerOccurrences",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimeTickers",
                schema: "ticker",
                table: "TimeTickers",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_CronTickers",
                schema: "ticker",
                table: "CronTickers",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_CronTickerOccurrences",
                schema: "ticker",
                table: "CronTickerOccurrences",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_OutboxState",
                table: "OutboxState",
                column: "OutboxId"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_OutboxMessage",
                table: "OutboxMessage",
                column: "SequenceNumber"
            );

            migrationBuilder.AddUniqueConstraint(
                name: "AK_InboxState_MessageId_ConsumerId",
                table: "InboxState",
                columns: new[] { "MessageId", "ConsumerId" }
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_InboxState",
                table: "InboxState",
                column: "Id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_TimeTicker_Status_ExecutionTime",
                schema: "ticker",
                table: "TimeTickers",
                columns: new[] { "Status", "ExecutionTime", "Request" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Function_Expression_Request",
                schema: "ticker",
                table: "CronTickers",
                columns: new[] { "Function", "Expression", "Request" },
                unique: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_CronTickerOccurrences_CronTickers_CronTickerId",
                schema: "ticker",
                table: "CronTickerOccurrences",
                column: "CronTickerId",
                principalSchema: "ticker",
                principalTable: "CronTickers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId",
                table: "OutboxMessage",
                columns: new[] { "InboxMessageId", "InboxConsumerId" },
                principalTable: "InboxState",
                principalColumns: new[] { "MessageId", "ConsumerId" }
            );

            migrationBuilder.AddForeignKey(
                name: "FK_OutboxMessage_OutboxState_OutboxId",
                table: "OutboxMessage",
                column: "OutboxId",
                principalTable: "OutboxState",
                principalColumn: "OutboxId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_TimeTickers_TimeTickers_ParentId",
                schema: "ticker",
                table: "TimeTickers",
                column: "ParentId",
                principalSchema: "ticker",
                principalTable: "TimeTickers",
                principalColumn: "Id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CronTickerOccurrences_CronTickers_CronTickerId",
                schema: "ticker",
                table: "CronTickerOccurrences"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId",
                table: "OutboxMessage"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_OutboxMessage_OutboxState_OutboxId",
                table: "OutboxMessage"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_TimeTickers_TimeTickers_ParentId",
                schema: "ticker",
                table: "TimeTickers"
            );

            migrationBuilder.DropPrimaryKey(
                name: "PK_TimeTickers",
                schema: "ticker",
                table: "TimeTickers"
            );

            migrationBuilder.DropIndex(
                name: "IX_TimeTicker_Status_ExecutionTime",
                schema: "ticker",
                table: "TimeTickers"
            );

            migrationBuilder.DropPrimaryKey(
                name: "PK_CronTickers",
                schema: "ticker",
                table: "CronTickers"
            );

            migrationBuilder.DropIndex(
                name: "IX_Function_Expression_Request",
                schema: "ticker",
                table: "CronTickers"
            );

            migrationBuilder.DropPrimaryKey(
                name: "PK_CronTickerOccurrences",
                schema: "ticker",
                table: "CronTickerOccurrences"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_OutboxState", table: "OutboxState");

            migrationBuilder.DropPrimaryKey(name: "PK_OutboxMessage", table: "OutboxMessage");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_InboxState_MessageId_ConsumerId",
                table: "InboxState"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_InboxState", table: "InboxState");

            migrationBuilder.DropColumn(
                name: "ExceptionMessage",
                schema: "ticker",
                table: "TimeTickers"
            );

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "ticker",
                table: "CronTickerOccurrences"
            );

            migrationBuilder.DropColumn(
                name: "ExceptionMessage",
                schema: "ticker",
                table: "CronTickerOccurrences"
            );

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "ticker",
                table: "CronTickerOccurrences"
            );

            migrationBuilder.RenameTable(name: "OutboxState", newName: "outbox_state");

            migrationBuilder.RenameTable(name: "OutboxMessage", newName: "outbox_message");

            migrationBuilder.RenameTable(name: "InboxState", newName: "inbox_state");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "ticker",
                table: "TimeTickers",
                newName: "status"
            );

            migrationBuilder.RenameColumn(
                name: "Retries",
                schema: "ticker",
                table: "TimeTickers",
                newName: "retries"
            );

            migrationBuilder.RenameColumn(
                name: "Request",
                schema: "ticker",
                table: "TimeTickers",
                newName: "request"
            );

            migrationBuilder.RenameColumn(
                name: "Function",
                schema: "ticker",
                table: "TimeTickers",
                newName: "function"
            );

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "ticker",
                table: "TimeTickers",
                newName: "description"
            );

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "ticker",
                table: "TimeTickers",
                newName: "id"
            );

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "ticker",
                table: "TimeTickers",
                newName: "updated_at"
            );

            migrationBuilder.RenameColumn(
                name: "RetryIntervals",
                schema: "ticker",
                table: "TimeTickers",
                newName: "retry_intervals"
            );

            migrationBuilder.RenameColumn(
                name: "RetryCount",
                schema: "ticker",
                table: "TimeTickers",
                newName: "retry_count"
            );

            migrationBuilder.RenameColumn(
                name: "LockedAt",
                schema: "ticker",
                table: "TimeTickers",
                newName: "locked_at"
            );

            migrationBuilder.RenameColumn(
                name: "LockHolder",
                schema: "ticker",
                table: "TimeTickers",
                newName: "lock_holder"
            );

            migrationBuilder.RenameColumn(
                name: "InitIdentifier",
                schema: "ticker",
                table: "TimeTickers",
                newName: "init_identifier"
            );

            migrationBuilder.RenameColumn(
                name: "ExecutionTime",
                schema: "ticker",
                table: "TimeTickers",
                newName: "execution_time"
            );

            migrationBuilder.RenameColumn(
                name: "ExecutedAt",
                schema: "ticker",
                table: "TimeTickers",
                newName: "executed_at"
            );

            migrationBuilder.RenameColumn(
                name: "ElapsedTime",
                schema: "ticker",
                table: "TimeTickers",
                newName: "elapsed_time"
            );

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "ticker",
                table: "TimeTickers",
                newName: "created_at"
            );

            migrationBuilder.RenameColumn(
                name: "SkippedReason",
                schema: "ticker",
                table: "TimeTickers",
                newName: "exception"
            );

            migrationBuilder.RenameColumn(
                name: "RunCondition",
                schema: "ticker",
                table: "TimeTickers",
                newName: "batch_run_condition"
            );

            migrationBuilder.RenameColumn(
                name: "ParentId",
                schema: "ticker",
                table: "TimeTickers",
                newName: "batch_parent"
            );

            migrationBuilder.RenameIndex(
                name: "IX_TimeTickers_ParentId",
                schema: "ticker",
                table: "TimeTickers",
                newName: "ix_time_tickers_batch_parent"
            );

            migrationBuilder.RenameColumn(
                name: "Retries",
                schema: "ticker",
                table: "CronTickers",
                newName: "retries"
            );

            migrationBuilder.RenameColumn(
                name: "Request",
                schema: "ticker",
                table: "CronTickers",
                newName: "request"
            );

            migrationBuilder.RenameColumn(
                name: "Function",
                schema: "ticker",
                table: "CronTickers",
                newName: "function"
            );

            migrationBuilder.RenameColumn(
                name: "Expression",
                schema: "ticker",
                table: "CronTickers",
                newName: "expression"
            );

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "ticker",
                table: "CronTickers",
                newName: "description"
            );

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "ticker",
                table: "CronTickers",
                newName: "id"
            );

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "ticker",
                table: "CronTickers",
                newName: "updated_at"
            );

            migrationBuilder.RenameColumn(
                name: "RetryIntervals",
                schema: "ticker",
                table: "CronTickers",
                newName: "retry_intervals"
            );

            migrationBuilder.RenameColumn(
                name: "InitIdentifier",
                schema: "ticker",
                table: "CronTickers",
                newName: "init_identifier"
            );

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "ticker",
                table: "CronTickers",
                newName: "created_at"
            );

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "ticker",
                table: "CronTickerOccurrences",
                newName: "status"
            );

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "ticker",
                table: "CronTickerOccurrences",
                newName: "id"
            );

            migrationBuilder.RenameColumn(
                name: "RetryCount",
                schema: "ticker",
                table: "CronTickerOccurrences",
                newName: "retry_count"
            );

            migrationBuilder.RenameColumn(
                name: "LockedAt",
                schema: "ticker",
                table: "CronTickerOccurrences",
                newName: "locked_at"
            );

            migrationBuilder.RenameColumn(
                name: "LockHolder",
                schema: "ticker",
                table: "CronTickerOccurrences",
                newName: "lock_holder"
            );

            migrationBuilder.RenameColumn(
                name: "ExecutionTime",
                schema: "ticker",
                table: "CronTickerOccurrences",
                newName: "execution_time"
            );

            migrationBuilder.RenameColumn(
                name: "ExecutedAt",
                schema: "ticker",
                table: "CronTickerOccurrences",
                newName: "executed_at"
            );

            migrationBuilder.RenameColumn(
                name: "ElapsedTime",
                schema: "ticker",
                table: "CronTickerOccurrences",
                newName: "elapsed_time"
            );

            migrationBuilder.RenameColumn(
                name: "CronTickerId",
                schema: "ticker",
                table: "CronTickerOccurrences",
                newName: "cron_ticker_id"
            );

            migrationBuilder.RenameColumn(
                name: "SkippedReason",
                schema: "ticker",
                table: "CronTickerOccurrences",
                newName: "exception"
            );

            migrationBuilder.RenameColumn(
                name: "Delivered",
                table: "outbox_state",
                newName: "delivered"
            );

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "outbox_state",
                newName: "created"
            );

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "outbox_state",
                newName: "row_version"
            );

            migrationBuilder.RenameColumn(
                name: "LockId",
                table: "outbox_state",
                newName: "lock_id"
            );

            migrationBuilder.RenameColumn(
                name: "LastSequenceNumber",
                table: "outbox_state",
                newName: "last_sequence_number"
            );

            migrationBuilder.RenameColumn(
                name: "OutboxId",
                table: "outbox_state",
                newName: "outbox_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_OutboxState_Created",
                table: "outbox_state",
                newName: "ix_outbox_state_created"
            );

            migrationBuilder.RenameColumn(
                name: "Properties",
                table: "outbox_message",
                newName: "properties"
            );

            migrationBuilder.RenameColumn(
                name: "Headers",
                table: "outbox_message",
                newName: "headers"
            );

            migrationBuilder.RenameColumn(name: "Body", table: "outbox_message", newName: "body");

            migrationBuilder.RenameColumn(
                name: "SourceAddress",
                table: "outbox_message",
                newName: "source_address"
            );

            migrationBuilder.RenameColumn(
                name: "SentTime",
                table: "outbox_message",
                newName: "sent_time"
            );

            migrationBuilder.RenameColumn(
                name: "ResponseAddress",
                table: "outbox_message",
                newName: "response_address"
            );

            migrationBuilder.RenameColumn(
                name: "RequestId",
                table: "outbox_message",
                newName: "request_id"
            );

            migrationBuilder.RenameColumn(
                name: "OutboxId",
                table: "outbox_message",
                newName: "outbox_id"
            );

            migrationBuilder.RenameColumn(
                name: "MessageType",
                table: "outbox_message",
                newName: "message_type"
            );

            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "outbox_message",
                newName: "message_id"
            );

            migrationBuilder.RenameColumn(
                name: "InitiatorId",
                table: "outbox_message",
                newName: "initiator_id"
            );

            migrationBuilder.RenameColumn(
                name: "InboxMessageId",
                table: "outbox_message",
                newName: "inbox_message_id"
            );

            migrationBuilder.RenameColumn(
                name: "InboxConsumerId",
                table: "outbox_message",
                newName: "inbox_consumer_id"
            );

            migrationBuilder.RenameColumn(
                name: "FaultAddress",
                table: "outbox_message",
                newName: "fault_address"
            );

            migrationBuilder.RenameColumn(
                name: "ExpirationTime",
                table: "outbox_message",
                newName: "expiration_time"
            );

            migrationBuilder.RenameColumn(
                name: "EnqueueTime",
                table: "outbox_message",
                newName: "enqueue_time"
            );

            migrationBuilder.RenameColumn(
                name: "DestinationAddress",
                table: "outbox_message",
                newName: "destination_address"
            );

            migrationBuilder.RenameColumn(
                name: "CorrelationId",
                table: "outbox_message",
                newName: "correlation_id"
            );

            migrationBuilder.RenameColumn(
                name: "ConversationId",
                table: "outbox_message",
                newName: "conversation_id"
            );

            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "outbox_message",
                newName: "content_type"
            );

            migrationBuilder.RenameColumn(
                name: "SequenceNumber",
                table: "outbox_message",
                newName: "sequence_number"
            );

            migrationBuilder.RenameIndex(
                name: "IX_OutboxMessage_OutboxId_SequenceNumber",
                table: "outbox_message",
                newName: "ix_outbox_message_outbox_id_sequence_number"
            );

            migrationBuilder.RenameIndex(
                name: "IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber",
                table: "outbox_message",
                newName: "ix_outbox_message_inbox_message_id_inbox_consumer_id_sequence_"
            );

            migrationBuilder.RenameIndex(
                name: "IX_OutboxMessage_ExpirationTime",
                table: "outbox_message",
                newName: "ix_outbox_message_expiration_time"
            );

            migrationBuilder.RenameIndex(
                name: "IX_OutboxMessage_EnqueueTime",
                table: "outbox_message",
                newName: "ix_outbox_message_enqueue_time"
            );

            migrationBuilder.RenameColumn(
                name: "Received",
                table: "inbox_state",
                newName: "received"
            );

            migrationBuilder.RenameColumn(
                name: "Delivered",
                table: "inbox_state",
                newName: "delivered"
            );

            migrationBuilder.RenameColumn(
                name: "Consumed",
                table: "inbox_state",
                newName: "consumed"
            );

            migrationBuilder.RenameColumn(name: "Id", table: "inbox_state", newName: "id");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "inbox_state",
                newName: "row_version"
            );

            migrationBuilder.RenameColumn(
                name: "ReceiveCount",
                table: "inbox_state",
                newName: "receive_count"
            );

            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "inbox_state",
                newName: "message_id"
            );

            migrationBuilder.RenameColumn(name: "LockId", table: "inbox_state", newName: "lock_id");

            migrationBuilder.RenameColumn(
                name: "LastSequenceNumber",
                table: "inbox_state",
                newName: "last_sequence_number"
            );

            migrationBuilder.RenameColumn(
                name: "ExpirationTime",
                table: "inbox_state",
                newName: "expiration_time"
            );

            migrationBuilder.RenameColumn(
                name: "ConsumerId",
                table: "inbox_state",
                newName: "consumer_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_InboxState_Delivered",
                table: "inbox_state",
                newName: "ix_inbox_state_delivered"
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "execution_time",
                schema: "ticker",
                table: "TimeTickers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_time_tickers",
                schema: "ticker",
                table: "TimeTickers",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_cron_tickers",
                schema: "ticker",
                table: "CronTickers",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_cron_ticker_occurrences",
                schema: "ticker",
                table: "CronTickerOccurrences",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_outbox_state",
                table: "outbox_state",
                column: "outbox_id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_outbox_message",
                table: "outbox_message",
                column: "sequence_number"
            );

            migrationBuilder.AddUniqueConstraint(
                name: "ak_inbox_state_message_id_consumer_id",
                table: "inbox_state",
                columns: new[] { "message_id", "consumer_id" }
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_inbox_state",
                table: "inbox_state",
                column: "id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_TimeTicker_Status_ExecutionTime",
                schema: "ticker",
                table: "TimeTickers",
                columns: new[] { "status", "execution_time" }
            );

            migrationBuilder.AddForeignKey(
                name: "fk_cron_ticker_occurrences_cron_tickers_cron_ticker_id",
                schema: "ticker",
                table: "CronTickerOccurrences",
                column: "cron_ticker_id",
                principalSchema: "ticker",
                principalTable: "CronTickers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_outbox_message_inbox_state_inbox_message_id_inbox_consumer_",
                table: "outbox_message",
                columns: new[] { "inbox_message_id", "inbox_consumer_id" },
                principalTable: "inbox_state",
                principalColumns: new[] { "message_id", "consumer_id" }
            );

            migrationBuilder.AddForeignKey(
                name: "fk_outbox_message_outbox_state_outbox_id",
                table: "outbox_message",
                column: "outbox_id",
                principalTable: "outbox_state",
                principalColumn: "outbox_id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_time_tickers_time_tickers_batch_parent",
                schema: "ticker",
                table: "TimeTickers",
                column: "batch_parent",
                principalSchema: "ticker",
                principalTable: "TimeTickers",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull
            );
        }
    }
}
