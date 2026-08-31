const fs = require('fs');
const path = require('path');
const puppeteer = require('puppeteer-core');
const { marked } = require('marked');

const EDGE_PATH = "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe";
const ROOT_DIR = path.join(__dirname, '..');
const DOCS_DIR = path.join(__dirname);
const INPUT_MD = path.join(DOCS_DIR, 'Lab_Report.md');
const OUTPUT_PDF = path.join(DOCS_DIR, 'Final_Lab_Report_Hotel_Management_System.pdf');
const LOGO_PATH = path.join(DOCS_DIR, 'JUST_LOGO.png');

function getLogoBase64() {
  if (fs.existsSync(LOGO_PATH)) {
    const buffer = fs.readFileSync(LOGO_PATH);
    return `data:image/png;base64,${buffer.toString('base64')}`;
  }
  return '';
}

function getHtmlTemplate(contentHtml, logoBase64) {
  return `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <title>Lab Report on Hotel Management System - Shafikul Islam Marwan</title>
  <script src="https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.min.js"></script>
  <style>
    @page {
      size: A4;
      margin: 14mm 16mm 16mm 16mm;
    }

    @page:first {
      margin: 0 !important;
    }

    * {
      box-sizing: border-box;
      -webkit-print-color-adjust: exact !important;
      print-color-adjust: exact !important;
    }

    body {
      font-family: 'Segoe UI', -apple-system, BlinkMacSystemFont, 'Inter', Roboto, 'Helvetica Neue', Arial, sans-serif;
      font-size: 10pt;
      line-height: 1.48;
      color: #0f172a;
      background-color: #ffffff;
      margin: 0;
      padding: 0;
    }

    /* Official Academic Cover Page */
    .cover-page {
      height: 100vh;
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      align-items: center;
      text-align: center;
      page-break-after: always;
      padding: 36px 30px 28px 30px;
      border: 3px double #1e3a8a;
      box-sizing: border-box;
      background: #ffffff;
    }

    .cover-header {
      display: flex;
      flex-direction: column;
      align-items: center;
      margin-top: 5px;
    }

    .cover-logo {
      height: 95px;
      width: auto;
      margin-bottom: 12px;
      object-fit: contain;
    }

    .cover-univ {
      font-size: 18pt;
      font-weight: 800;
      color: #1e3a8a;
      letter-spacing: 0.8px;
      text-transform: uppercase;
      margin-bottom: 4px;
    }

    .cover-dept {
      font-size: 13pt;
      color: #1f2937;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      margin-bottom: 6px;
    }

    .cover-course {
      font-size: 12pt;
      color: #2563eb;
      font-weight: 700;
    }

    .cover-body {
      margin: auto 0;
      width: 100%;
    }

    .cover-title {
      font-size: 24pt;
      font-weight: 800;
      color: #000000;
      margin: 6px 0;
      line-height: 1.2;
    }

    .cover-project {
      font-size: 15pt;
      font-weight: 700;
      color: #000000;
      margin-top: 8px;
    }

    .cover-subtitle {
      font-size: 11pt;
      color: #374151;
      max-width: 620px;
      margin: 10px auto 0 auto;
      line-height: 1.45;
    }

    .cover-footer {
      width: 100%;
      display: flex;
      justify-content: space-between;
      text-align: left;
      border-top: 2px solid #cbd5e1;
      padding-top: 14px;
      margin-bottom: 8px;
    }

    .cover-card {
      width: 48%;
      background: #f8fafc;
      border: 1px solid #cbd5e1;
      border-top: 3px solid #2563eb;
      border-radius: 6px;
      padding: 10px 14px;
    }

    .cover-card-title {
      font-size: 9.5pt;
      font-weight: 800;
      color: #1e3a8a;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      border-bottom: 1px solid #e2e8f0;
      padding-bottom: 3px;
      margin-bottom: 6px;
    }

    .cover-card-text {
      font-size: 10pt;
      color: #111827;
      line-height: 1.4;
    }

    .cover-date-box {
      width: 100%;
      text-align: center;
      font-size: 10.5pt;
      font-weight: 700;
      color: #1e3a8a;
      margin-top: 4px;
    }

    /* Content Styling */
    .content-wrapper {
      padding: 4px 2px;
    }

    h1 {
      font-size: 15.5pt;
      font-weight: 800;
      color: #0f172a;
      border-left: 5px solid #2563eb;
      padding-left: 10px;
      padding-bottom: 2px;
      margin-top: 16px;
      margin-bottom: 8px;
      page-break-after: avoid;
    }

    h2 {
      font-size: 13pt;
      font-weight: 700;
      color: #1e3a8a;
      border-bottom: 1.5px solid #e2e8f0;
      padding-bottom: 2px;
      margin-top: 14px;
      margin-bottom: 6px;
      page-break-after: avoid;
    }

    h3 {
      font-size: 11pt;
      font-weight: 700;
      color: #2563eb;
      margin-top: 12px;
      margin-bottom: 4px;
      page-break-after: avoid;
    }

    h4 {
      font-size: 10.5pt;
      font-weight: 700;
      color: #374151;
      margin-top: 8px;
      margin-bottom: 3px;
      page-break-after: avoid;
    }

    p {
      margin: 0 0 7px 0;
      text-align: justify;
      text-justify: inter-word;
    }

    ul, ol {
      margin: 0 0 8px 0;
      padding-left: 20px;
    }

    li {
      margin-bottom: 2px;
      text-align: justify;
    }

    /* Compact 2-Column Table of Contents (Fits on Exactly Page 2) */
    .toc-page-container {
      page-break-after: always;
      padding: 10px 0;
    }

    .toc-container {
      background: #fdfdfd;
      border: 1px solid #e2e8f0;
      border-radius: 6px;
      padding: 12px 16px;
    }

    .toc-title {
      font-size: 14pt;
      font-weight: 800;
      color: #1e3a8a;
      border-bottom: 2px solid #2563eb;
      padding-bottom: 4px;
      margin-bottom: 10px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .toc-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      column-gap: 20px;
      row-gap: 1px;
    }

    .toc-item {
      display: flex;
      justify-content: space-between;
      align-items: baseline;
      font-size: 8.5pt;
      line-height: 1.35;
      margin: 1px 0;
    }

    .toc-item-h1 {
      font-weight: 700;
      color: #0f172a;
      margin-top: 4px;
      border-bottom: 1px solid #f1f5f9;
    }

    .toc-item-h2 {
      padding-left: 8px;
      color: #1e3a8a;
    }

    .toc-item-h3 {
      padding-left: 16px;
      color: #475569;
      font-size: 8pt;
    }

    .toc-item a {
      color: inherit;
      text-decoration: none;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
      max-width: 82%;
    }

    .toc-item a:hover {
      color: #2563eb;
      text-decoration: underline;
    }

    .toc-dots {
      flex-grow: 1;
      border-bottom: 1px dotted #cbd5e1;
      margin: 0 4px;
      height: 1px;
    }

    .toc-page {
      font-weight: 700;
      color: #1e3a8a;
      flex-shrink: 0;
      font-size: 8.5pt;
    }

    /* Tables */
    table {
      width: 100%;
      border-collapse: collapse;
      margin: 8px 0 10px 0;
      font-size: 8.5pt;
    }

    th, td {
      border: 1px solid #cbd5e1;
      padding: 4px 6px;
      text-align: left;
      vertical-align: top;
    }

    th {
      background-color: #1e3a8a !important;
      color: #ffffff !important;
      font-weight: 700;
      letter-spacing: 0.3px;
    }

    tr:nth-child(even) {
      background-color: #f8fafc !important;
    }

    /* Code Blocks & ASCII Tables */
    pre {
      background-color: #0f172a !important;
      color: #f8fafc !important;
      padding: 6px 10px;
      border-radius: 4px;
      font-family: 'Consolas', 'Courier New', monospace;
      font-size: 7.5pt;
      line-height: 1.3;
      overflow-x: auto;
      margin: 8px 0;
      border: 1px solid #334155;
    }

    code {
      font-family: 'Consolas', 'Courier New', monospace;
      font-size: 8.5pt;
      background-color: #f1f5f9;
      color: #0f172a;
      padding: 1px 3px;
      border-radius: 3px;
    }

    pre code {
      background-color: transparent !important;
      color: inherit !important;
      padding: 0;
    }

    /* Mermaid Diagrams - Sized comfortably, crisp and fully legible */
    .mermaid {
      text-align: center;
      margin: 8px auto;
      padding: 6px;
      background-color: #ffffff;
      border: 1px solid #e2e8f0;
      border-radius: 6px;
    }

    .mermaid svg {
      width: 100% !important;
      max-width: 95% !important;
      max-height: 420px !important;
      height: auto !important;
      display: block;
      margin: 0 auto;
    }

    /* Screenshots - Sized comfortably, crisp and clear */
    .doc-image-container {
      text-align: center;
      margin: 8px 0;
    }

    .doc-image {
      max-width: 92%;
      max-height: 320px;
      height: auto;
      border: 1px solid #cbd5e1;
      border-radius: 6px;
      box-shadow: 0 2px 6px rgba(0,0,0,0.08);
      display: block;
      margin: 0 auto 3px auto;
    }

    .image-caption {
      font-size: 8pt;
      font-style: italic;
      color: #475569;
      font-weight: 600;
    }

    /* Academic GANTT Schedule Table */
    .gantt-table {
      width: 100%;
      border-collapse: collapse;
      margin: 8px 0;
      font-size: 8.5pt;
    }

    .gantt-table th {
      background-color: #1e3a8a !important;
      color: #ffffff !important;
      text-align: center;
      padding: 5px 4px;
      font-weight: 700;
      border: 1px solid #94a3b8;
    }

    .gantt-table td {
      border: 1px solid #cbd5e1;
      padding: 4px 6px;
      vertical-align: middle;
    }

    .gantt-phase {
      font-weight: 700;
      color: #0f172a;
      background-color: #f8fafc;
      width: 32%;
    }

    .gantt-dates {
      font-size: 8pt;
      color: #475569;
      text-align: center;
      width: 20%;
    }

    .gantt-bar-cell {
      padding: 2px;
      width: 48%;
    }

    .gantt-bar {
      display: block;
      height: 16px;
      border-radius: 3px;
      color: #ffffff;
      font-size: 7.5pt;
      font-weight: 700;
      line-height: 16px;
      text-align: center;
      letter-spacing: 0.3px;
    }

    .bar-p1 { background-color: #3b82f6; width: 100%; }
    .bar-p2 { background-color: #6366f1; width: 100%; }
    .bar-p3 { background-color: #8b5cf6; width: 100%; }
    .bar-p4 { background-color: #ec4899; width: 100%; }
    .bar-p5 { background-color: #f59e0b; width: 100%; }
    .bar-p6 { background-color: #10b981; width: 100%; }

    hr {
      border: 0;
      border-top: 1px solid #cbd5e1;
      margin: 12px 0;
    }
  </style>
</head>
<body>

  <!-- Official Academic Cover Page (Page 1) -->
  <div class="cover-page">
    <div class="cover-header">
      ${logoBase64 ? `<img src="${logoBase64}" alt="JUST Logo" class="cover-logo">` : ''}
      <div class="cover-univ">Jashore University of Science and Technology</div>
      <div class="cover-dept">Department of Computer Science and Engineering</div>
      <div class="cover-course">Course: Software Development Project-II</div>
    </div>

    <div class="cover-body">
      <div class="cover-title">Lab Report on Hotel Management System</div>
      <div class="cover-project">Project: The Haunted Hotel Management System</div>
      <div class="cover-subtitle">Full-Stack Web Application with ASP .NET Core (.NET 10) Clean Architecture and React 19 SPA</div>
    </div>

    <div class="cover-footer">
      <div class="cover-card">
        <div class="cover-card-title">SUBMITTED BY</div>
        <div class="cover-card-text">
          <strong>Shafikul Islam Marwan</strong><br>
          Student ID: <strong>220121</strong><br>
          Registration No: <strong>2201020</strong><br>
          Degree: B.Sc. in Computer Science and Engineering<br>
          Department of CSE, JUST
        </div>
      </div>
      <div class="cover-card">
        <div class="cover-card-title">SUBMITTED TO</div>
        <div class="cover-card-text">
          <strong>Dr. Md. Nasim Adnan</strong><br>
          Assistant Professor<br>
          Department of Computer Science and Engineering<br>
          Jashore University of Science and Technology (JUST)
        </div>
      </div>
    </div>

    <div class="cover-date-box">
      Date of Submission: 01-September-2026
    </div>
  </div>

  <!-- Main Document Body (Page 2 onward) -->
  <div class="content-wrapper">
    ${contentHtml}
  </div>

  <script>
    mermaid.initialize({
      startOnLoad: true,
      theme: 'neutral',
      securityLevel: 'loose',
      fontSize: 12,
      fontFamily: 'Segoe UI, Arial, sans-serif'
    });
  </script>
</body>
</html>`;
}

function generateAcademicGanttTableHtml() {
  return `
  <table class="gantt-table">
    <thead>
      <tr>
        <th>SDLC Project Phase & Milestone</th>
        <th>Duration & Timeline</th>
        <th>Schedule Timeline Visualization (Weeks 1 to 16)</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td class="gantt-phase">Phase 1: Planning & Requirements Analysis</td>
        <td class="gantt-dates">Weeks 1 - 2 (Sep 01 - Sep 15, 2025)</td>
        <td class="gantt-bar-cell"><span class="gantt-bar bar-p1">Weeks 1 - 2 [Planning & Feasibility]</span></td>
      </tr>
      <tr>
        <td class="gantt-phase">Phase 2: Architectural Design & UML Modeling</td>
        <td class="gantt-dates">Weeks 3 - 5 (Sep 15 - Oct 06, 2025)</td>
        <td class="gantt-bar-cell"><span class="gantt-bar bar-p2">Weeks 3 - 5 [UML & Schema Design]</span></td>
      </tr>
      <tr>
        <td class="gantt-phase">Phase 3: Backend Development & REST APIs</td>
        <td class="gantt-dates">Weeks 6 - 9 (Oct 07 - Nov 02, 2025)</td>
        <td class="gantt-bar-cell"><span class="gantt-bar bar-p3">Weeks 6 - 9 [.NET 10 & EF Core]</span></td>
      </tr>
      <tr>
        <td class="gantt-phase">Phase 4: Frontend SPA & Dashboard Integration</td>
        <td class="gantt-dates">Weeks 10 - 12 (Nov 03 - Nov 25, 2025)</td>
        <td class="gantt-bar-cell"><span class="gantt-bar bar-p4">Weeks 10 - 12 [React 19 & Charts]</span></td>
      </tr>
      <tr>
        <td class="gantt-phase">Phase 5: Automated Testing & Verification</td>
        <td class="gantt-dates">Weeks 13 - 14 (Nov 26 - Dec 07, 2025)</td>
        <td class="gantt-bar-cell"><span class="gantt-bar bar-p5">Weeks 13 - 14 [xUnit Suite (35 Tests)]</span></td>
      </tr>
      <tr>
        <td class="gantt-phase">Phase 6: Final Documentation & Defense</td>
        <td class="gantt-dates">Weeks 15 - 16 (Dec 08 - Dec 23, 2025)</td>
        <td class="gantt-bar-cell"><span class="gantt-bar bar-p6">Weeks 15 - 16 [Lab Report & Defense]</span></td>
      </tr>
    </tbody>
  </table>
  `;
}

function processMarkdownAndExtractMermaid(mdContent) {
  const mermaidDiagrams = [];

  // Replace GANTT chart codeblock with clean academic HTML table
  let preprocessed = mdContent.replace(/```mermaid\s*gantt[\s\S]*?```/g, () => {
    return generateAcademicGanttTableHtml();
  });

  // Extract other Mermaid codeblocks before marked.parse
  preprocessed = preprocessed.replace(/```mermaid\s*([\s\S]*?)```/g, (match, code) => {
    const index = mermaidDiagrams.length;
    mermaidDiagrams.push(code.trim());
    return `<!-- MERMAID_DIAGRAM_PLACEHOLDER_${index} -->`;
  });

  // Convert markdown images to styled base64 images
  preprocessed = preprocessed.replace(/!\[(.*?)\]\((.*?)\)/g, (match, alt, src) => {
    let cleanSrc = src.trim();
    let possiblePaths = [
      cleanSrc,
      path.join(ROOT_DIR, cleanSrc.replace(/^\.\//, '')),
      path.join(DOCS_DIR, cleanSrc.replace(/^\.\//, '')),
      path.join(DOCS_DIR, 'screenshot', path.basename(cleanSrc)),
      path.join(ROOT_DIR, 'docs', 'screenshot', path.basename(cleanSrc))
    ];

    let fullImgPath = possiblePaths.find(p => fs.existsSync(p));

    if (fullImgPath && fs.existsSync(fullImgPath)) {
      const ext = path.extname(fullImgPath).toLowerCase().replace('.', '');
      const mime = ext === 'png' ? 'image/png' : ext === 'jpg' || ext === 'jpeg' ? 'image/jpeg' : 'image/png';
      const imgBuffer = fs.readFileSync(fullImgPath);
      const base64Data = `data:${mime};base64,${imgBuffer.toString('base64')}`;
      return `\n\n<div class="doc-image-container"><img src="${base64Data}" alt="${alt}" class="doc-image"><div class="image-caption">Figure: ${alt}</div></div>\n\n`;
    }
    return `\n\n<div class="doc-image-container"><em>[Screenshot: ${alt}]</em></div>\n\n`;
  });

  // Remove existing raw markdown table of contents if present (we will generate an academic clickable TOC with exact page numbers)
  preprocessed = preprocessed.replace(/## Table of Contents[\s\S]*?(?=---)/, '<!-- TOC_PLACEHOLDER -->');

  // Parse markdown to HTML
  let html = marked.parse(preprocessed);

  // Restore raw unescaped Mermaid diagrams inside <div class="mermaid">
  mermaidDiagrams.forEach((code, index) => {
    const placeholder = `<!-- MERMAID_DIAGRAM_PLACEHOLDER_${index} -->`;
    const mermaidDiv = `<div class="mermaid">\n${code}\n</div>`;
    html = html.replace(placeholder, mermaidDiv);
  });

  return html;
}

// Table of Contents definition with anchor IDs and section hierarchy
const tocItems = [
  { id: "1-introduction", title: "1. Introduction", level: 1 },
  { id: "11-problem-statement", title: "1.1 Problem Statement", level: 2 },
  { id: "12-case-study-with-problem-identification", title: "1.2 Case Study with Problem Identification", level: 2 },
  { id: "13-specification-of-overall-high-level-goals", title: "1.3 Specification of Overall High-Level Goals", level: 2 },
  { id: "2-feasibility-analysis", title: "2. Feasibility Analysis", level: 1 },
  { id: "21-technical-feasibility", title: "2.1 Technical Feasibility", level: 2 },
  { id: "22-operational-feasibility", title: "2.2 Operational Feasibility", level: 2 },
  { id: "23-economic-feasibility-and-cost-benefit-analysis", title: "2.3 Economic Feasibility and Cost-Benefit Analysis", level: 2 },
  { id: "24-project-schedule-and-gantt-chart", title: "2.4 Project Schedule and GANTT Chart", level: 2 },
  { id: "3-business-requirement-analysis", title: "3. Business Requirement Analysis", level: 1 },
  { id: "31-information-gathering", title: "3.1 Information Gathering", level: 2 },
  { id: "32-goals-and-objectives", title: "3.2 Goals and Objectives", level: 2 },
  { id: "33-detailed-business-processes", title: "3.3 Detailed Business Processes", level: 2 },
  { id: "34-stakeholder-identification", title: "3.4 Stakeholder Identification", level: 2 },
  { id: "35-scope-definition", title: "3.5 Scope Definition (In-Scope and Out-of-Scope)", level: 2 },
  { id: "36-requirements-validation-matrix", title: "3.6 Requirements Validation Matrix", level: 2 },
  { id: "4-software-requirements-specification-srs", title: "4. Software Requirements Specification (SRS)", level: 1 },
  { id: "41-functional-requirements", title: "4.1 Functional Requirements", level: 2 },
  { id: "42-non-functional-requirements", title: "4.2 Non-Functional Requirements", level: 2 },
  { id: "43-system-models-and-diagrams-using-uml", title: "4.3 System Models and Diagrams using UML", level: 2 },
  { id: "431-use-case-diagram", title: "4.3.1 Use Case Diagram", level: 3 },
  { id: "432-class-diagram-domain-model", title: "4.3.2 Class Diagram (Domain Model)", level: 3 },
  { id: "433-sequence-diagram-booking-creation", title: "4.3.3 Sequence Diagram: Booking Creation", level: 3 },
  { id: "434-sequence-diagram-payment-and-status-promotion", title: "4.3.4 Sequence Diagram: Payment and Status Promotion", level: 3 },
  { id: "435-activity-diagram-guest-lifecycle", title: "4.3.5 Activity Diagram: Guest Lifecycle", level: 3 },
  { id: "436-entity-relationship-diagram-erd", title: "4.3.6 Entity-Relationship Diagram (ERD)", level: 3 },
  { id: "437-relational-database-schema-specifications", title: "4.3.7 Relational Database Schema Specifications", level: 3 },
  { id: "438-data-flow-diagram-level-0-context-diagram", title: "4.3.8 Data Flow Diagram: Level 0 (Context Diagram)", level: 3 },
  { id: "439-data-flow-diagram-level-1-decomposition-diagram", title: "4.3.9 Data Flow Diagram: Level 1 (Decomposition Diagram)", level: 3 },
  { id: "5-software-development", title: "5. Software Development", level: 1 },
  { id: "51-backend-architectural-design-clean-architecture", title: "5.1 Backend Architectural Design (Clean Architecture)", level: 2 },
  { id: "52-dependency-injection-and-security-pipeline", title: "5.2 Dependency Injection and Security Pipeline", level: 2 },
  { id: "53-categorized-restful-api-endpoints", title: "5.3 Categorized RESTful API Endpoints", level: 2 },
  { id: "54-global-exception-handling-and-error-pipeline", title: "5.4 Global Exception Handling and Error Pipeline", level: 2 },
  { id: "55-frontend-uxui-design-system-and-state-pipeline", title: "5.5 Frontend UX/UI Design System and State Pipeline", level: 2 },
  { id: "56-user-interface-implementations-and-screenshots", title: "5.6 User Interface Implementations and Screenshots", level: 2 },
  { id: "6-software-testing", title: "6. Software Testing", level: 1 },
  { id: "61-testing-methodology-and-framework", title: "6.1 Testing Methodology and Framework", level: 2 },
  { id: "62-test-suite-breakdown-and-verification-matrix", title: "6.2 Test Suite Breakdown and Verification Matrix", level: 2 },
  { id: "7-software-implementation-and-deployment", title: "7. Software Implementation and Deployment", level: 1 },
  { id: "71-prerequisites", title: "7.1 Prerequisites", level: 2 },
  { id: "72-backend-setup-and-database-migrations", title: "7.2 Backend Setup and Database Migrations", level: 2 },
  { id: "73-frontend-setup-and-development-execution", title: "7.3 Frontend Setup and Development Execution", level: 2 },
  { id: "74-verification-and-test-execution", title: "7.4 Verification and Test Execution", level: 2 },
  { id: "8-conclusion-and-future-scopes", title: "8. Conclusion and Future Scopes", level: 1 }
];

function buildTocHtml(pageMap) {
  let tocHtml = `
  <div class="toc-page-container">
    <div class="toc-container">
      <div class="toc-title">Table of Contents</div>
      <div class="toc-grid">
  `;

  tocItems.forEach(item => {
    const pageNum = pageMap && pageMap[item.id] ? pageMap[item.id] : 2;
    const levelClass = item.level === 1 ? 'toc-item-h1' : item.level === 2 ? 'toc-item-h2' : 'toc-item-h3';
    tocHtml += `
      <div class="toc-item ${levelClass}">
        <a href="#${item.id}">${item.title}</a>
        <div class="toc-dots"></div>
        <span class="toc-page">${pageNum}</span>
      </div>
    `;
  });

  tocHtml += `
      </div>
    </div>
  </div>`;
  return tocHtml;
}

async function buildLabReportPdf() {
  console.log('Reading Lab_Report.md...');
  const rawMarkdown = fs.readFileSync(INPUT_MD, 'utf8');

  console.log('Processing Markdown and extracting diagrams...');
  let processedHtml = processMarkdownAndExtractMermaid(rawMarkdown);

  // Initial dummy TOC placeholder for First Pass calculation
  let initialHtml = processedHtml.replace('<!-- TOC_PLACEHOLDER -->', buildTocHtml(null));
  const logoBase64 = getLogoBase64();
  let fullHtml = getHtmlTemplate(initialHtml, logoBase64);

  const tempHtmlPath = path.join(DOCS_DIR, 'temp_lab_report.html');
  fs.writeFileSync(tempHtmlPath, fullHtml, 'utf8');

  console.log('Launching Headless Edge Browser for Pass 1 (Exact Page Number Calculation)...');
  const browser = await puppeteer.launch({
    executablePath: EDGE_PATH,
    headless: true,
    args: ['--no-sandbox', '--disable-setuid-sandbox', '--disable-gpu']
  });

  const page = await browser.newPage();
  await page.goto(`file://${tempHtmlPath}`, { waitUntil: 'networkidle0', timeout: 90000 });

  // Run Mermaid render
  await page.evaluate(async () => {
    if (window.mermaid) {
      await window.mermaid.run();
    }
  });
  await new Promise(resolve => setTimeout(resolve, 3500));

  // Compute exact page numbers for each heading in Pass 1
  const pageMap = await page.evaluate((items) => {
    const map = {};
    const pageHeightPx = 1009; // A4 height (267mm content at 96 DPI)
    const coverPageEl = document.querySelector('.cover-page');
    const coverHeight = coverPageEl ? coverPageEl.offsetHeight : 1009;

    items.forEach(item => {
      let el = document.getElementById(item.id);
      if (!el) {
        // Fallback search by text match
        const headers = Array.from(document.querySelectorAll('h1, h2, h3, h4'));
        el = headers.find(h => h.innerText.trim().toLowerCase().startsWith(item.title.toLowerCase().substring(0, 8)));
      }

      if (el) {
        const top = el.getBoundingClientRect().top + window.scrollY;
        if (top <= coverHeight) {
          map[item.id] = 1;
        } else {
          const contentTop = top - coverHeight;
          const pageIndex = Math.floor(contentTop / pageHeightPx) + 2;
          map[item.id] = pageIndex;
        }
      } else {
        map[item.id] = 2;
      }
    });
    return map;
  }, tocItems);

  console.log('Pass 1 Computed Page Numbers for Table of Contents:', pageMap);

  // Pass 2: Re-inject the exact Table of Contents with computed page numbers
  console.log('Building Pass 2 HTML with exact Table of Contents page numbers and clickable links...');
  const finalTocHtml = buildTocHtml(pageMap);
  const finalContentHtml = processedHtml.replace('<!-- TOC_PLACEHOLDER -->', finalTocHtml);
  const finalFullHtml = getHtmlTemplate(finalContentHtml, logoBase64);
  fs.writeFileSync(tempHtmlPath, finalFullHtml, 'utf8');

  await page.goto(`file://${tempHtmlPath}`, { waitUntil: 'networkidle0', timeout: 90000 });
  await page.evaluate(async () => {
    if (window.mermaid) {
      await window.mermaid.run();
    }
  });
  await new Promise(resolve => setTimeout(resolve, 3500));

  console.log('Writing Final Publication PDF to:', OUTPUT_PDF);
  await page.pdf({
    path: OUTPUT_PDF,
    format: 'A4',
    printBackground: true,
    margin: {
      top: '14mm',
      bottom: '16mm',
      left: '16mm',
      right: '16mm'
    },
    displayHeaderFooter: true,
    headerTemplate: '<div></div>',
    footerTemplate: `
      <style>
        .footer-box {
          font-size: 9pt;
          color: #64748b;
          width: 100%;
          text-align: center;
          font-family: 'Segoe UI', Arial, sans-serif;
        }
      </style>
      <div class="footer-box">
        <span class="pageNumber"></span>
      </div>
    `
  });

  console.log('PDF Generation Complete!');
  await page.close();
  await browser.close();

  // Clean up temporary HTML
  if (fs.existsSync(tempHtmlPath)) {
    fs.unlinkSync(tempHtmlPath);
  }
}

buildLabReportPdf().catch(err => {
  console.error('Error during Lab Report PDF generation:', err);
  process.exit(1);
});
