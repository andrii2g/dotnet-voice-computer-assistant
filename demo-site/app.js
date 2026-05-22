const projects = [
  {
    name: "dotnet-voice-computer-assistant",
    tags: ["dotnet", "accessibility", "automation"],
    status: "Prototype",
    updated: "2026-05-22",
    difficulty: "Medium",
    summary: "A prompt-first Computer Use assistant that will later accept voice commands."
  },
  {
    name: "greenhouse-sensor-dashboard",
    tags: ["iot", "greens", "dashboard"],
    status: "Idea",
    updated: "2026-05-15",
    difficulty: "Easy",
    summary: "A small IoT dashboard for temperature, humidity, and grow-light monitoring."
  },
  {
    name: "spool-inventory-helper",
    tags: ["3d-printing", "inventory", "utility"],
    status: "Ready",
    updated: "2026-05-10",
    difficulty: "Easy",
    summary: "Track filament spools, weights, colors, and reorder thresholds."
  },
  {
    name: "github-issue-triage-lite",
    tags: ["github", "automation", "dotnet"],
    status: "Prototype",
    updated: "2026-05-18",
    difficulty: "Medium",
    summary: "A local-only mock issue triage board for testing UI agents safely."
  },
  {
    name: "kitchen-stock-labeler",
    tags: ["household", "labels", "utility"],
    status: "Idea",
    updated: "2026-05-07",
    difficulty: "Easy",
    summary: "Generate printable labels and QR codes for home pantry containers."
  }
];

const elements = {
  searchInput: document.getElementById("searchInput"),
  tagFilter: document.getElementById("tagFilter"),
  sortSelect: document.getElementById("sortSelect"),
  projectList: document.getElementById("projectList"),
  detailsPanel: document.getElementById("detailsPanel"),
  statusLabel: document.getElementById("statusLabel")
};

const state = {
  searchText: "",
  tag: "",
  sort: "updated-desc",
  selectedProjectName: null
};

function initialize() {
  populateTagFilter();
  bindEvents();
  renderProjects();
  showProjectDetails(projects[0].name);
}

function populateTagFilter() {
  const tags = [...new Set(projects.flatMap(project => project.tags))].sort((left, right) => left.localeCompare(right));

  for (const tag of tags) {
    const option = document.createElement("option");
    option.value = tag;
    option.textContent = tag;
    elements.tagFilter.append(option);
  }
}

function bindEvents() {
  elements.searchInput.addEventListener("input", event => {
    state.searchText = event.target.value.trim().toLowerCase();
    renderProjects();
  });

  elements.tagFilter.addEventListener("change", event => {
    state.tag = event.target.value;
    renderProjects();
  });

  elements.sortSelect.addEventListener("change", event => {
    state.sort = event.target.value;
    renderProjects();
  });
}

function applyFilters() {
  const filtered = projects.filter(project => {
    const haystack = [project.name, project.summary, ...project.tags].join(" ").toLowerCase();
    const matchesSearch = state.searchText === "" || haystack.includes(state.searchText);
    const matchesTag = state.tag === "" || project.tags.includes(state.tag);
    return matchesSearch && matchesTag;
  });

  return sortProjects(filtered, state.sort);
}

function sortProjects(items, sortMode) {
  const sorted = [...items];

  switch (sortMode) {
    case "name-asc":
      sorted.sort((left, right) => left.name.localeCompare(right.name));
      break;
    case "status-asc":
      sorted.sort((left, right) => left.status.localeCompare(right.status));
      break;
    case "difficulty-asc":
      sorted.sort((left, right) => left.difficulty.localeCompare(right.difficulty));
      break;
    case "updated-desc":
    default:
      sorted.sort((left, right) => right.updated.localeCompare(left.updated));
      break;
  }

  return sorted;
}

function renderProjects() {
  const visibleProjects = applyFilters();
  elements.projectList.innerHTML = "";

  if (visibleProjects.length === 0) {
    const emptyState = document.createElement("article");
    emptyState.className = "project-card";
    emptyState.innerHTML = `
      <h3>No matching projects</h3>
      <p class="project-summary">Adjust the search text or tag filter to see projects again.</p>
    `;
    elements.projectList.append(emptyState);
    elements.statusLabel.textContent = `Showing 0 of ${projects.length} projects`;
    showEmptyDetails();
    return;
  }

  for (const project of visibleProjects) {
    elements.projectList.append(createProjectCard(project));
  }

  elements.statusLabel.textContent = `Showing ${visibleProjects.length} of ${projects.length} projects`;

  const selectedStillVisible = visibleProjects.some(project => project.name === state.selectedProjectName);
  if (!selectedStillVisible) {
    showProjectDetails(visibleProjects[0].name);
  }
}

function createProjectCard(project) {
  const article = document.createElement("article");
  article.className = "project-card";
  article.setAttribute("data-project-name", project.name);

  article.innerHTML = `
    <h3>${project.name}</h3>
    <p class="project-summary">${project.summary}</p>
    <div class="meta-grid">
      <div class="meta-item">
        <span class="meta-label">Status</span>
        <span class="meta-value">${project.status}</span>
      </div>
      <div class="meta-item">
        <span class="meta-label">Updated</span>
        <span class="meta-value">${project.updated}</span>
      </div>
      <div class="meta-item">
        <span class="meta-label">Difficulty</span>
        <span class="meta-value">${project.difficulty}</span>
      </div>
      <div class="meta-item">
        <span class="meta-label">Tag count</span>
        <span class="meta-value">${project.tags.length}</span>
      </div>
    </div>
    <div class="tag-list">
      ${project.tags.map(tag => `<span class="tag-chip">${tag}</span>`).join("")}
    </div>
  `;

  const button = document.createElement("button");
  button.className = "details-button";
  button.type = "button";
  button.textContent = "Open details";
  button.addEventListener("click", () => showProjectDetails(project.name));

  article.append(button);
  return article;
}

function showProjectDetails(projectName) {
  const project = projects.find(candidate => candidate.name === projectName);
  if (!project) {
    showEmptyDetails();
    return;
  }

  state.selectedProjectName = project.name;

  elements.detailsPanel.innerHTML = `
    <h2>Project details</h2>
    <p>${project.summary}</p>
    <dl>
      <div>
        <dt>Name</dt>
        <dd>${project.name}</dd>
      </div>
      <div>
        <dt>Status</dt>
        <dd>${project.status}</dd>
      </div>
      <div>
        <dt>Updated</dt>
        <dd>${project.updated}</dd>
      </div>
      <div>
        <dt>Difficulty</dt>
        <dd>${project.difficulty}</dd>
      </div>
      <div>
        <dt>Tags</dt>
        <dd>${project.tags.join(", ")}</dd>
      </div>
    </dl>
  `;
}

function showEmptyDetails() {
  state.selectedProjectName = null;
  elements.detailsPanel.innerHTML = `
    <h2>Project details</h2>
    <p>No project is currently selected.</p>
  `;
}

initialize();
