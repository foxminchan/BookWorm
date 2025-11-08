using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWorm.Scheduler.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropTickerQEfData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId",
                table: "OutboxMessage"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_OutboxMessage_OutboxState_OutboxId",
                table: "OutboxMessage"
            );

            migrationBuilder.DropTable(name: "CronTickerOccurrences", schema: "ticker");

            migrationBuilder.DropTable(name: "TimeTickers", schema: "ticker");

            migrationBuilder.DropTable(name: "CronTickers", schema: "ticker");

            migrationBuilder.DropPrimaryKey(name: "PK_OutboxState", table: "OutboxState");

            migrationBuilder.DropPrimaryKey(name: "PK_OutboxMessage", table: "OutboxMessage");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_InboxState_MessageId_ConsumerId",
                table: "InboxState"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_InboxState", table: "InboxState");

            migrationBuilder.RenameTable(name: "OutboxState", newName: "outbox_state");

            migrationBuilder.RenameTable(name: "OutboxMessage", newName: "outbox_message");

            migrationBuilder.RenameTable(name: "InboxState", newName: "inbox_state");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_outbox_message_inbox_state_inbox_message_id_inbox_consumer_",
                table: "outbox_message"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_outbox_message_outbox_state_outbox_id",
                table: "outbox_message"
            );

            migrationBuilder.DropPrimaryKey(name: "pk_outbox_state", table: "outbox_state");

            migrationBuilder.DropPrimaryKey(name: "pk_outbox_message", table: "outbox_message");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_inbox_state_message_id_consumer_id",
                table: "inbox_state"
            );

            migrationBuilder.DropPrimaryKey(name: "pk_inbox_state", table: "inbox_state");

            migrationBuilder.EnsureSchema(name: "ticker");

            migrationBuilder.RenameTable(name: "outbox_state", newName: "OutboxState");

            migrationBuilder.RenameTable(name: "outbox_message", newName: "OutboxMessage");

            migrationBuilder.RenameTable(name: "inbox_state", newName: "InboxState");

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

            migrationBuilder.CreateTable(
                name: "CronTickers",
                schema: "ticker",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Expression = table.Column<string>(type: "text", nullable: true),
                    Function = table.Column<string>(type: "text", nullable: true),
                    InitIdentifier = table.Column<string>(type: "text", nullable: true),
                    Request = table.Column<byte[]>(type: "bytea", nullable: true),
                    Retries = table.Column<int>(type: "integer", nullable: false),
                    RetryIntervals = table.Column<int[]>(type: "integer[]", nullable: true),
                    UpdatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CronTickers", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "TimeTickers",
                schema: "ticker",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ElapsedTime = table.Column<long>(type: "bigint", nullable: false),
                    ExceptionMessage = table.Column<string>(type: "text", nullable: true),
                    ExecutedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    ExecutionTime = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    Function = table.Column<string>(type: "text", nullable: true),
                    InitIdentifier = table.Column<string>(type: "text", nullable: true),
                    LockHolder = table.Column<string>(type: "text", nullable: true),
                    LockedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    Request = table.Column<byte[]>(type: "bytea", nullable: true),
                    Retries = table.Column<int>(type: "integer", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    RetryIntervals = table.Column<int[]>(type: "integer[]", nullable: true),
                    RunCondition = table.Column<int>(type: "integer", nullable: true),
                    SkippedReason = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeTickers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeTickers_TimeTickers_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "ticker",
                        principalTable: "TimeTickers",
                        principalColumn: "Id"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "CronTickerOccurrences",
                schema: "ticker",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CronTickerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    ElapsedTime = table.Column<long>(type: "bigint", nullable: false),
                    ExceptionMessage = table.Column<string>(type: "text", nullable: true),
                    ExecutedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    ExecutionTime = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    LockHolder = table.Column<string>(type: "text", nullable: true),
                    LockedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedReason = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CronTickerOccurrences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CronTickerOccurrences_CronTickers_CronTickerId",
                        column: x => x.CronTickerId,
                        principalSchema: "ticker",
                        principalTable: "CronTickers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_CronTickerOccurrence_CronTickerId",
                schema: "ticker",
                table: "CronTickerOccurrences",
                column: "CronTickerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_CronTickerOccurrence_ExecutionTime",
                schema: "ticker",
                table: "CronTickerOccurrences",
                column: "ExecutionTime"
            );

            migrationBuilder.CreateIndex(
                name: "IX_CronTickerOccurrence_Status_ExecutionTime",
                schema: "ticker",
                table: "CronTickerOccurrences",
                columns: new[] { "Status", "ExecutionTime" }
            );

            migrationBuilder.CreateIndex(
                name: "UQ_CronTickerId_ExecutionTime",
                schema: "ticker",
                table: "CronTickerOccurrences",
                columns: new[] { "CronTickerId", "ExecutionTime" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_CronTickers_Expression",
                schema: "ticker",
                table: "CronTickers",
                column: "Expression"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Function_Expression_Request",
                schema: "ticker",
                table: "CronTickers",
                columns: new[] { "Function", "Expression", "Request" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_TimeTicker_ExecutionTime",
                schema: "ticker",
                table: "TimeTickers",
                column: "ExecutionTime"
            );

            migrationBuilder.CreateIndex(
                name: "IX_TimeTicker_Status_ExecutionTime",
                schema: "ticker",
                table: "TimeTickers",
                columns: new[] { "Status", "ExecutionTime", "Request" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_TimeTickers_ParentId",
                schema: "ticker",
                table: "TimeTickers",
                column: "ParentId"
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
        }
    }
}
