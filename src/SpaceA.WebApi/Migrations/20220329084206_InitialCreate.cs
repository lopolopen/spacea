using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SpaceA.WebApi.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "member_capacities",
                columns: table => new
                {
                    team_id = table.Column<uint>(nullable: false),
                    iteration_id = table.Column<uint>(nullable: false),
                    member_id = table.Column<uint>(nullable: false),
                    type = table.Column<int>(nullable: false),
                    hours_per_day = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_member_capacities", x => new { x.team_id, x.iteration_id, x.member_id, x.type });
                });

            migrationBuilder.CreateTable(
                name: "members",
                columns: table => new
                {
                    id = table.Column<uint>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    account_name = table.Column<string>(maxLength: 32, nullable: false),
                    first_name = table.Column<string>(maxLength: 16, nullable: true),
                    last_name = table.Column<string>(maxLength: 16, nullable: true),
                    xing = table.Column<string>(maxLength: 2, nullable: true),
                    ming = table.Column<string>(maxLength: 2, nullable: true),
                    avatar_uid = table.Column<string>(maxLength: 8, nullable: true),
                    disabled = table.Column<bool>(nullable: false),
                    refresh_token = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_members", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "project_histories",
                columns: table => new
                {
                    id = table.Column<uint>(nullable: false),
                    rev = table.Column<int>(nullable: false),
                    owner_id = table.Column<uint>(nullable: false),
                    name = table.Column<string>(maxLength: 32, nullable: false),
                    desc = table.Column<string>(maxLength: 256, nullable: true),
                    created_date = table.Column<DateTime>(nullable: false),
                    default_team_id = table.Column<uint>(nullable: true),
                    root_folder_id = table.Column<uint>(nullable: true),
                    root_iteration_id = table.Column<Guid>(nullable: true),
                    deleted_flag = table.Column<Guid>(nullable: false),
                    changed_date = table.Column<DateTime>(nullable: false),
                    changer_id = table.Column<uint>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_histories", x => new { x.id, x.rev });
                });

            migrationBuilder.CreateTable(
                name: "remaining_works",
                columns: table => new
                {
                    team_id = table.Column<uint>(nullable: false),
                    iteration_id = table.Column<uint>(nullable: false),
                    work_item_type = table.Column<int>(nullable: false),
                    accounting_date = table.Column<DateTime>(nullable: false),
                    estimated_hours = table.Column<float>(nullable: false),
                    remaining_hours = table.Column<float>(nullable: false),
                    completed_hours = table.Column<float>(nullable: false),
                    remaining_count = table.Column<int>(nullable: false),
                    completed_count = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_remaining_works", x => new { x.team_id, x.iteration_id, x.work_item_type, x.accounting_date });
                });

            migrationBuilder.CreateTable(
                name: "repos",
                columns: table => new
                {
                    id = table.Column<uint>(nullable: false),
                    name = table.Column<string>(nullable: true),
                    name_with_namespace = table.Column<string>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: true),
                    web_url = table.Column<string>(nullable: true),
                    http_url_to_repo = table.Column<string>(nullable: true),
                    ssh_url_to_repo = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    namespace_name = table.Column<string>(nullable: true),
                    namespace_path = table.Column<string>(nullable: true),
                    namespace_full_path = table.Column<string>(nullable: true),
                    namespace_kind = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "work_item_histories",
                columns: table => new
                {
                    id = table.Column<uint>(nullable: false),
                    rev = table.Column<int>(nullable: false),
                    type = table.Column<int>(nullable: false),
                    title = table.Column<string>(maxLength: 128, nullable: false),
                    assignee_id = table.Column<uint>(nullable: true),
                    desc = table.Column<string>(nullable: true),
                    accept_criteria = table.Column<string>(nullable: true),
                    repro_steps = table.Column<string>(nullable: true),
                    priority = table.Column<int>(nullable: false),
                    state = table.Column<int>(nullable: false),
                    reason = table.Column<string>(maxLength: 128, nullable: true),
                    folder_id = table.Column<uint>(nullable: false),
                    iteration_id = table.Column<uint>(nullable: true),
                    upload_files = table.Column<string>(nullable: true),
                    estimated_time = table.Column<float>(nullable: true),
                    estimated_hours = table.Column<float>(nullable: true),
                    remaining_hours = table.Column<float>(nullable: true),
                    completed_hours = table.Column<float>(nullable: true),
                    environment = table.Column<string>(nullable: true),
                    severity = table.Column<int>(nullable: true),
                    parent_id = table.Column<uint>(nullable: true),
                    order = table.Column<string>(maxLength: 512, nullable: true),
                    created_date = table.Column<DateTime>(nullable: false),
                    creator_id = table.Column<uint>(nullable: false),
                    project_id = table.Column<uint>(nullable: false),
                    changed_date = table.Column<DateTime>(nullable: false),
                    changer_id = table.Column<uint>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_item_histories", x => new { x.id, x.rev });
                });

            migrationBuilder.CreateTable(
                name: "configs",
                columns: table => new
                {
                    MemberId = table.Column<uint>(nullable: false),
                    Key = table.Column<string>(maxLength: 64, nullable: false),
                    value = table.Column<string>(nullable: true),
                    is_shared = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configs", x => new { x.MemberId, x.Key });
                    table.ForeignKey(
                        name: "FK_configs_members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "groups",
                columns: table => new
                {
                    id = table.Column<uint>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    account_name = table.Column<string>(maxLength: 32, nullable: false),
                    name = table.Column<string>(maxLength: 32, nullable: true),
                    acronym = table.Column<string>(maxLength: 4, nullable: true),
                    desc = table.Column<string>(maxLength: 256, nullable: true),
                    disabled = table.Column<bool>(nullable: false, defaultValue: false),
                    leader_id = table.Column<uint>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_groups", x => x.id);
                    table.ForeignKey(
                        name: "FK_groups_members_leader_id",
                        column: x => x.leader_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "personal_access_tokens",
                columns: table => new
                {
                    Token = table.Column<string>(maxLength: 28, nullable: false),
                    created_at = table.Column<DateTime>(nullable: false),
                    expired_at = table.Column<DateTime>(nullable: true),
                    owner_id = table.Column<uint>(nullable: false),
                    remarks = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personal_access_tokens", x => x.Token);
                    table.ForeignKey(
                        name: "FK_personal_access_tokens_members_owner_id",
                        column: x => x.owner_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "group_members",
                columns: table => new
                {
                    group_id = table.Column<uint>(nullable: false),
                    member_id = table.Column<uint>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group_members", x => new { x.group_id, x.member_id });
                    table.ForeignKey(
                        name: "FK_group_members_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_group_members_members_member_id",
                        column: x => x.member_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<uint>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    rev = table.Column<int>(nullable: false),
                    owner_id = table.Column<uint>(nullable: false),
                    name = table.Column<string>(maxLength: 32, nullable: false),
                    desc = table.Column<string>(maxLength: 256, nullable: true),
                    avatar_uid = table.Column<string>(maxLength: 32, nullable: true),
                    created_date = table.Column<DateTime>(nullable: false),
                    default_team_id = table.Column<uint>(nullable: true),
                    root_folder_id = table.Column<uint>(nullable: true),
                    root_iteration_id = table.Column<uint>(nullable: true),
                    deleted_flag = table.Column<Guid>(nullable: false),
                    changed_date = table.Column<DateTime>(nullable: false),
                    changer_id = table.Column<uint>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.id);
                    table.ForeignKey(
                        name: "FK_projects_members_owner_id",
                        column: x => x.owner_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "folders",
                columns: table => new
                {
                    id = table.Column<uint>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(maxLength: 128, nullable: false),
                    path = table.Column<string>(maxLength: 512, nullable: true),
                    project_id = table.Column<uint>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_folders", x => x.id);
                    table.ForeignKey(
                        name: "FK_folders_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "iterations",
                columns: table => new
                {
                    id = table.Column<uint>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(maxLength: 128, nullable: false),
                    path = table.Column<string>(maxLength: 512, nullable: true),
                    start_date = table.Column<DateTime>(type: "date", nullable: true),
                    end_date = table.Column<DateTime>(type: "date", nullable: true),
                    project_id = table.Column<uint>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_iterations", x => x.id);
                    table.ForeignKey(
                        name: "FK_iterations_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_repos",
                columns: table => new
                {
                    project_id = table.Column<uint>(nullable: false),
                    repo_id = table.Column<uint>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_repos", x => new { x.project_id, x.repo_id });
                    table.ForeignKey(
                        name: "FK_project_repos_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_repos_repos_repo_id",
                        column: x => x.repo_id,
                        principalTable: "repos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "teams",
                columns: table => new
                {
                    id = table.Column<uint>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(maxLength: 32, nullable: false),
                    acronym = table.Column<string>(maxLength: 4, nullable: true),
                    desc = table.Column<string>(maxLength: 256, nullable: true),
                    project_id = table.Column<uint>(nullable: false),
                    default_folder_id = table.Column<uint>(nullable: true),
                    default_iteration_id = table.Column<uint>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teams", x => x.id);
                    table.ForeignKey(
                        name: "FK_teams_folders_default_folder_id",
                        column: x => x.default_folder_id,
                        principalTable: "folders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_teams_iterations_default_iteration_id",
                        column: x => x.default_iteration_id,
                        principalTable: "iterations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_teams_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "work_items",
                columns: table => new
                {
                    id = table.Column<uint>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    rev = table.Column<int>(nullable: false, defaultValue: 0),
                    type = table.Column<int>(nullable: false),
                    title = table.Column<string>(maxLength: 128, nullable: false),
                    assignee_id = table.Column<uint>(nullable: true),
                    desc = table.Column<string>(nullable: true),
                    accept_criteria = table.Column<string>(nullable: true),
                    repro_steps = table.Column<string>(nullable: true),
                    priority = table.Column<int>(nullable: false),
                    state = table.Column<int>(nullable: false),
                    reason = table.Column<string>(maxLength: 128, nullable: true),
                    folder_id = table.Column<uint>(nullable: false),
                    iteration_id = table.Column<uint>(nullable: true),
                    upload_files = table.Column<string>(nullable: true),
                    estimated_time = table.Column<float>(nullable: true),
                    estimated_hours = table.Column<float>(nullable: true),
                    remaining_hours = table.Column<float>(nullable: true),
                    completed_hours = table.Column<float>(nullable: true),
                    environment = table.Column<string>(nullable: true),
                    severity = table.Column<int>(nullable: true),
                    parent_id = table.Column<uint>(nullable: true),
                    order = table.Column<string>(maxLength: 512, nullable: true),
                    created_date = table.Column<DateTime>(nullable: false),
                    creator_id = table.Column<uint>(nullable: false),
                    project_id = table.Column<uint>(nullable: false),
                    changed_date = table.Column<DateTime>(nullable: false),
                    changer_id = table.Column<uint>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_work_items_members_assignee_id",
                        column: x => x.assignee_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_work_items_members_changer_id",
                        column: x => x.changer_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_work_items_members_creator_id",
                        column: x => x.creator_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_work_items_folders_folder_id",
                        column: x => x.folder_id,
                        principalTable: "folders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_work_items_iterations_iteration_id",
                        column: x => x.iteration_id,
                        principalTable: "iterations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_work_items_work_items_parent_id",
                        column: x => x.parent_id,
                        principalTable: "work_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_work_items_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "team_folders",
                columns: table => new
                {
                    team_id = table.Column<uint>(nullable: false),
                    folder_id = table.Column<uint>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team_folders", x => new { x.team_id, x.folder_id });
                    table.ForeignKey(
                        name: "FK_team_folders_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_team_folders_folders_folder_id",
                        column: x => x.folder_id,
                        principalTable: "folders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "team_iterations",
                columns: table => new
                {
                    team_id = table.Column<uint>(nullable: false),
                    iteration_id = table.Column<uint>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team_iterations", x => new { x.team_id, x.iteration_id });
                    table.ForeignKey(
                        name: "FK_team_iterations_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_team_iterations_iterations_iteration_id",
                        column: x => x.iteration_id,
                        principalTable: "iterations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "team_members",
                columns: table => new
                {
                    team_id = table.Column<uint>(nullable: false),
                    member_id = table.Column<uint>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team_members", x => new { x.team_id, x.member_id });
                    table.ForeignKey(
                        name: "FK_team_members_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_team_members_members_member_id",
                        column: x => x.member_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "attachments",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    file_name = table.Column<string>(maxLength: 256, nullable: false),
                    size = table.Column<long>(nullable: false),
                    uploaded_time = table.Column<DateTime>(nullable: true),
                    work_item_id = table.Column<uint>(nullable: true),
                    creator_id = table.Column<uint>(nullable: true),
                    comments = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attachments", x => x.id);
                    table.ForeignKey(
                        name: "FK_attachments_members_creator_id",
                        column: x => x.creator_id,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_attachments_work_items_work_item_id",
                        column: x => x.work_item_id,
                        principalTable: "work_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<uint>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    project_id = table.Column<uint>(nullable: true),
                    work_item_id = table.Column<uint>(nullable: true),
                    text = table.Column<string>(maxLength: 32, nullable: true),
                    color = table.Column<string>(maxLength: 16, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_tags_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tags_work_items_work_item_id",
                        column: x => x.work_item_id,
                        principalTable: "work_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attachments_creator_id",
                table: "attachments",
                column: "creator_id");

            migrationBuilder.CreateIndex(
                name: "IX_attachments_work_item_id",
                table: "attachments",
                column: "work_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_folders_path",
                table: "folders",
                column: "path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_folders_project_id",
                table: "folders",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_group_members_member_id",
                table: "group_members",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_groups_leader_id",
                table: "groups",
                column: "leader_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_iterations_path",
                table: "iterations",
                column: "path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_iterations_project_id",
                table: "iterations",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_member_capacities_team_id_iteration_id",
                table: "member_capacities",
                columns: new[] { "team_id", "iteration_id" });

            migrationBuilder.CreateIndex(
                name: "IX_members_account_name",
                table: "members",
                column: "account_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_personal_access_tokens_owner_id",
                table: "personal_access_tokens",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_repos_repo_id",
                table: "project_repos",
                column: "repo_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_default_team_id",
                table: "projects",
                column: "default_team_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_projects_owner_id",
                table: "projects",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_root_folder_id",
                table: "projects",
                column: "root_folder_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_projects_root_iteration_id",
                table: "projects",
                column: "root_iteration_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_projects_name_deleted_flag",
                table: "projects",
                columns: new[] { "name", "deleted_flag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_remaining_works_team_id_iteration_id",
                table: "remaining_works",
                columns: new[] { "team_id", "iteration_id" });

            migrationBuilder.CreateIndex(
                name: "IX_tags_project_id",
                table: "tags",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_tags_work_item_id",
                table: "tags",
                column: "work_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_folders_folder_id",
                table: "team_folders",
                column: "folder_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_iterations_iteration_id",
                table: "team_iterations",
                column: "iteration_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_members_member_id",
                table: "team_members",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_teams_default_folder_id",
                table: "teams",
                column: "default_folder_id");

            migrationBuilder.CreateIndex(
                name: "IX_teams_default_iteration_id",
                table: "teams",
                column: "default_iteration_id");

            migrationBuilder.CreateIndex(
                name: "IX_teams_project_id_name",
                table: "teams",
                columns: new[] { "project_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_work_items_assignee_id",
                table: "work_items",
                column: "assignee_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_items_changer_id",
                table: "work_items",
                column: "changer_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_items_creator_id",
                table: "work_items",
                column: "creator_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_items_folder_id",
                table: "work_items",
                column: "folder_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_items_iteration_id",
                table: "work_items",
                column: "iteration_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_items_parent_id",
                table: "work_items",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_items_project_id",
                table: "work_items",
                column: "project_id");

            migrationBuilder.AddForeignKey(
                name: "FK_projects_teams_default_team_id",
                table: "projects",
                column: "default_team_id",
                principalTable: "teams",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_projects_folders_root_folder_id",
                table: "projects",
                column: "root_folder_id",
                principalTable: "folders",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_projects_iterations_root_iteration_id",
                table: "projects",
                column: "root_iteration_id",
                principalTable: "iterations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_projects_members_owner_id",
                table: "projects");

            migrationBuilder.DropForeignKey(
                name: "FK_folders_projects_project_id",
                table: "folders");

            migrationBuilder.DropForeignKey(
                name: "FK_iterations_projects_project_id",
                table: "iterations");

            migrationBuilder.DropForeignKey(
                name: "FK_teams_projects_project_id",
                table: "teams");

            migrationBuilder.DropTable(
                name: "attachments");

            migrationBuilder.DropTable(
                name: "configs");

            migrationBuilder.DropTable(
                name: "group_members");

            migrationBuilder.DropTable(
                name: "member_capacities");

            migrationBuilder.DropTable(
                name: "personal_access_tokens");

            migrationBuilder.DropTable(
                name: "project_histories");

            migrationBuilder.DropTable(
                name: "project_repos");

            migrationBuilder.DropTable(
                name: "remaining_works");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "team_folders");

            migrationBuilder.DropTable(
                name: "team_iterations");

            migrationBuilder.DropTable(
                name: "team_members");

            migrationBuilder.DropTable(
                name: "work_item_histories");

            migrationBuilder.DropTable(
                name: "groups");

            migrationBuilder.DropTable(
                name: "repos");

            migrationBuilder.DropTable(
                name: "work_items");

            migrationBuilder.DropTable(
                name: "members");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "teams");

            migrationBuilder.DropTable(
                name: "folders");

            migrationBuilder.DropTable(
                name: "iterations");
        }
    }
}
