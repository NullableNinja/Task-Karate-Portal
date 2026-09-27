<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';

  type StaffAccessUser = {
    userId: string;
    displayName: string;
    email: string | null;
    isActive: boolean;
    canPublishDojoNews: boolean;
    roles: string[];
  };

  let users: StaffAccessUser[] = [];
  let loading = true;
  let savingId = '';
  let error = '';
  let notice = '';
  let creating = false;
  let createForm = { displayName: '', email: '', password: '', roles: ['Staff'] };
  const staffRoles = ['Administrator', 'Instructor', 'Staff'];

  async function load() {
    loading = true;
    error = '';
    try { users = await api<StaffAccessUser[]>('/api/portal-admin/staff-users'); }
    catch (e) { error = e instanceof Error ? e.message : 'Could not load staff access.'; }
    finally { loading = false; }
  }

  async function setNewsAccess(user: StaffAccessUser, enabled: boolean) {
    savingId = user.userId;
    error = '';
    notice = '';
    try {
      await api(`/api/portal-admin/staff-users/${user.userId}/news-access`, { method: 'PUT', body: JSON.stringify({ enabled }) });
      users = users.map((item) => item.userId === user.userId ? { ...item, canPublishDojoNews: enabled } : item);
      notice = `${user.displayName} can ${enabled ? 'now' : 'no longer'} manage Dojo News.`;
    } catch (e) { error = e instanceof Error ? e.message : 'Could not update staff access.'; }
    finally { savingId = ''; }
  }

  async function setActive(user: StaffAccessUser, active: boolean) {
    savingId = `${user.userId}-active`; error = ''; notice = '';
    try { await api(`/api/portal-admin/staff-users/${user.userId}/active`, { method: 'PUT', body: JSON.stringify({ active }) }); users = users.map((item) => item.userId === user.userId ? { ...item, isActive: active } : item); notice = `${user.displayName} is ${active ? 'active' : 'deactivated'}.`; }
    catch (e) { error = e instanceof Error ? e.message : 'Could not update staff status.'; }
    finally { savingId = ''; }
  }

  async function setRole(user: StaffAccessUser, role: string, enabled: boolean) {
    const roles = enabled ? [...new Set([...user.roles, role])] : user.roles.filter((item) => item !== role);
    if (!roles.some((item) => staffRoles.includes(item))) { error = 'A staff account must retain at least one staff role.'; return; }
    savingId = `${user.userId}-roles`; error = ''; notice = '';
    try { await api(`/api/portal-admin/staff-users/${user.userId}/roles`, { method: 'PUT', body: JSON.stringify({ roles }) }); users = users.map((item) => item.userId === user.userId ? { ...item, roles } : item); notice = `${user.displayName}'s staff roles were updated.`; }
    catch (e) { error = e instanceof Error ? e.message : 'Could not update staff roles.'; }
    finally { savingId = ''; }
  }

  async function createStaff() {
    creating = true; error = ''; notice = '';
    try {
      await api('/api/portal-admin/staff-users', { method: 'POST', body: JSON.stringify(createForm) });
      createForm = { displayName: '', email: '', password: '', roles: ['Staff'] };
      notice = 'Staff account created. Issue the credentials privately, then ask the staff member to sign in.';
      await load();
    } catch (e) { error = e instanceof Error ? e.message : 'Could not create staff account.'; }
    finally { creating = false; }
  }

  onMount(load);
</script>

<svelte:head><title>Staff · Access | Task Karate</title></svelte:head>

<div class="toolbar">
  <div><span class="eyebrow">STAFF ADMINISTRATION</span><h2>Staff access</h2><p class="muted">Manage Identity roles, active status, and the separate Dojo News publishing permission. Administrators always retain access.</p></div>
</div>
{#if error}<div class="error" role="alert">{error}</div>{/if}
{#if notice}<div class="status" role="status">{notice}</div>{/if}

<section class="card staff-access-card">
  <div class="section-heading"><div><h3>Create a staff login</h3><p class="muted">The initial password is entered here once and should be delivered privately. The staff member can use either the display name or email to sign in.</p></div></div>
  <form class="form-grid staff-create-form" on:submit|preventDefault={createStaff}><label>Display name<input bind:value={createForm.displayName} maxlength="120" required placeholder="Instructor Alex" /></label><label>Email<input type="email" bind:value={createForm.email} maxlength="240" required placeholder="alex@example.com" /></label><label>Initial password<input type="password" bind:value={createForm.password} minlength="12" required placeholder="Strong temporary password" /></label><label>Initial role<select bind:value={createForm.roles[0]}><option value="Staff">Staff</option><option value="Instructor">Instructor</option><option value="Administrator">Administrator</option></select></label><div class="full"><button class="button" disabled={creating || !createForm.displayName.trim() || !createForm.email.trim() || createForm.password.length < 12}>{creating ? 'Creating…' : 'Create staff login'}</button></div></form>
  <div class="section-heading staff-access-heading"><div><h3>Staff profiles</h3><p class="muted">Role changes affect access to the protected staff workspace. Deactivated accounts cannot sign in.</p></div><span class="pill">{users.length}</span></div>
  {#if loading}<p class="muted">Loading staff profiles…</p>{:else}
    <div class="staff-access-list">
      {#each users as user}
        <article class="staff-access-row">
          <div><strong>{user.displayName}</strong><small>{user.email ?? 'No email'} · {user.isActive ? 'Active' : 'Deactivated'}</small><div class="staff-role-controls">{#each staffRoles as role}<label><input type="checkbox" checked={user.roles.includes(role)} disabled={savingId === `${user.userId}-roles`} on:change={(event) => setRole(user, role, (event.currentTarget as HTMLInputElement).checked)} />{role}</label>{/each}</div></div>
          <div class="staff-access-actions"><label class="staff-access-toggle"><input type="checkbox" checked={user.canPublishDojoNews || user.roles.includes('Administrator')} disabled={savingId === user.userId || user.roles.includes('Administrator') || !user.isActive} on:change={(event) => setNewsAccess(user, (event.currentTarget as HTMLInputElement).checked)} /><span>Dojo News publisher</span></label><label class="staff-access-toggle"><input type="checkbox" checked={user.isActive} disabled={savingId === `${user.userId}-active`} on:change={(event) => setActive(user, (event.currentTarget as HTMLInputElement).checked)} /><span>Account active</span></label></div>
        </article>
      {:else}<p class="muted">No staff profiles are available.</p>{/each}
    </div>
  {/if}
</section>
