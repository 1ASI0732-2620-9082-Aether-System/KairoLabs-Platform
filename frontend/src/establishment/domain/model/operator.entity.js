/**
 * Operator entity within the Establishment bounded context.
 *
 * @class Operator
 */
export class Operator {
    constructor({
        id = null,
        alerts_answered = 0,
        schedule = '',
        establishment_id = null,
        users_id = null,
    } = {}) {
        this.id = id;
        this.alerts_answered = alerts_answered;
        this.schedule = schedule;
        this.establishment_id = establishment_id;
        this.users_id = users_id;
    }

    /** Normalizes any id value to a number or null. */
    static normalizeId(id) {
        if (id == null || id === '') return null;
        const n = Number(id);
        return Number.isNaN(n) ? null : n;
    }

    /**
     * Returns operator users that belong to the entity but have no /operators record yet.
     * @param {{ users: Object[], operators: Operator[], entityCode: string|null }} params
     */
    static getPendingUsers({ users, operators, entityCode }) {
        if (!entityCode) return [];
        const code = String(entityCode).trim().toUpperCase();
        const assignedUserIds = new Set(
            (operators || []).map((op) => Operator.normalizeId(op.users_id)).filter((id) => id != null),
        );
        return (users || []).filter((u) => {
            if (String(u.role || '').toUpperCase() !== 'OPERATOR') return false;
            if (String(u.entity_code || '').trim().toUpperCase() !== code) return false;
            return !assignedUserIds.has(Operator.normalizeId(u.id));
        });
    }

    /**
     * Returns operators assigned to establishments owned by a given admin.
     * @param {{ operators: Operator[], establishments: Object[], adminId: number|string|null }} params
     */
    static getAssignedForAdmin({ operators, establishments, adminId }) {
        if (adminId == null) return operators || [];
        const adminNum = Operator.normalizeId(adminId);
        const establishmentIds = new Set(
            (establishments || [])
                .filter((e) => Operator.normalizeId(e.admin_id) === adminNum)
                .map((e) => Operator.normalizeId(e.id)),
        );
        return (operators || []).filter((op) =>
            establishmentIds.has(Operator.normalizeId(op.establishment_id)),
        );
    }

    /** Formats a schedule value for display in lists and detail views. */
    static formatSchedule(schedule) {
        if (schedule == null || schedule === '') return '—';
        if (typeof schedule === 'string') return schedule;
        if (Array.isArray(schedule)) {
            if (!schedule.length) return '—';
            const first = schedule[0];
            const turn = first?.turn || first?.shift || '';
            const day = first?.day || first?.hours || first?.range || '';
            const line = [turn, day].filter(Boolean).join(' · ');
            if (!line) return '—';
            return schedule.length > 1 ? `${line} (+${schedule.length - 1})` : line;
        }
        if (typeof schedule === 'object') {
            const entries = Object.entries(schedule);
            if (!entries.length) return '—';
            const [turn, day] = entries[0];
            const line = [turn, day].filter(Boolean).join(' · ');
            return entries.length > 1 ? `${line} (+${entries.length - 1})` : line || '—';
        }
        return String(schedule);
    }
}
