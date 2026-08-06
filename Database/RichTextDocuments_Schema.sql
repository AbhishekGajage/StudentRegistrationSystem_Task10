-- =========================================================
-- Rich Text Editor module: document storage table.
-- Run this once against StudentRegistrationDB.
-- =========================================================

CREATE TABLE RichTextDocuments (
    DocumentID   INT IDENTITY(1,1) PRIMARY KEY,
    Title        NVARCHAR(200)   NOT NULL,
    Content      NVARCHAR(MAX)   NOT NULL,   -- sanitized HTML from the editor
    CreatedDate  DATETIME        NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME        NULL,
    CreatedBy    NVARCHAR(100)   NOT NULL,   -- admin username at time of creation
    Status       NVARCHAR(20)    NOT NULL DEFAULT 'Draft'  -- e.g. Draft / Published
);
GO

-- Speeds up the Title search on the document list page.
CREATE INDEX IX_RichTextDocuments_Title ON RichTextDocuments(Title);
GO
