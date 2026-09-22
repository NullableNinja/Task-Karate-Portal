export type Student = { id: string; firstName: string; lastName: string; preferredName?: string; birthDate?: string; ageGroup: string; joinDate: string; uniformSize?: string; beltSize?: string; isActive: boolean; beltRank?: string };
export type Guardian = { id: string; firstName: string; lastName: string; email: string; phone?: string; students: { id: string; name: string; relationship: string }[] };
export type Program = { id: string; name: string; description?: string; templates: Template[] };
export type Template = { id: string; name: string; dayOfWeek: string; startTime: string; durationMinutes: number; beltScope?: string };
export type Session = { id: string; classTemplateId: string; name: string; startTime: string; durationMinutes: number; sessionDateUtc: string; isCancelled: boolean };
export type Content = { id: string; title: string; body: string; status: string; publishedAtUtc?: string };
