import { useEffect, useState, useRef } from 'react';
import {
  Camera,
  Check,
  KeyRound,
  Lock,
  Mail,
  MapPin,
  Pencil,
  Phone,
  Shield,
  Trash2,
  User as UserIcon,
  X,
} from 'lucide-react';
import api from '../../api/axios';
import { useAuth } from '../../context/AuthContext';
import toast from 'react-hot-toast';
import './ProfileModal.css';

export default function ProfileModal({ open, onClose }) {
  const { user, updateUser } = useAuth();
  const fileInputRef = useRef(null);

  const [loading, setLoading] = useState(false);
  const [isEditing, setIsEditing] = useState(false);
  const [profile, setProfile] = useState(null);

  // Edit form state
  const [form, setForm] = useState({
    username: '',
    phoneNumber: '',
    address: '',
    profilePictureUrl: '',
  });
  const [saving, setSaving] = useState(false);

  // Password change state
  const [showPasswordSection, setShowPasswordSection] = useState(false);
  const [passwordForm, setPasswordForm] = useState({
    currentPassword: '',
    newPassword: '',
    confirmNewPassword: '',
  });
  const [passwordSaving, setPasswordSaving] = useState(false);

  // Fetch full profile when modal opens
  useEffect(() => {
    if (!open) return;
    async function loadProfile() {
      setLoading(true);
      try {
        const { data } = await api.get('/profile');
        setProfile(data);
        setForm({
          username: data.username || '',
          phoneNumber: data.phoneNumber || '',
          address: data.address || '',
          profilePictureUrl: data.profilePictureUrl || '',
        });
      } catch {
        // Fallback to auth context user
        if (user) {
          setProfile(user);
          setForm({
            username: user.username || '',
            phoneNumber: user.phoneNumber || '',
            address: user.address || '',
            profilePictureUrl: user.profilePictureUrl || '',
          });
        }
      } finally {
        setLoading(false);
      }
    }
    loadProfile();
    setIsEditing(false);
    setShowPasswordSection(false);
    setPasswordForm({ currentPassword: '', newPassword: '', confirmNewPassword: '' });
  }, [open, user]);

  if (!open) return null;

  function handlePhotoUpload(e) {
    const file = e.target.files?.[0];
    if (!file) return;

    if (file.size > 2 * 1024 * 1024) {
      toast.error('Image size must be under 2MB.');
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      setForm((prev) => ({ ...prev, profilePictureUrl: reader.result }));
    };
    reader.readAsDataURL(file);
  }

  function handleRemovePhoto() {
    setForm((prev) => ({ ...prev, profilePictureUrl: '' }));
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  }

  async function handleSaveProfile(e) {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = {
        username: form.username.trim(),
        phoneNumber: form.phoneNumber.trim(),
        address: form.address.trim(),
        profilePictureUrl: form.profilePictureUrl || null,
      };

      const { data } = await api.put('/profile', payload);
      setProfile(data);
      updateUser({
        username: data.username,
        phoneNumber: data.phoneNumber,
        address: data.address,
        profilePictureUrl: data.profilePictureUrl,
      });
      toast.success('Profile updated successfully.');
      setIsEditing(false);
    } catch (err) {
      toast.error(err.response?.data?.detail || 'Failed to update profile.');
    } finally {
      setSaving(false);
    }
  }

  async function handleChangePassword(e) {
    e.preventDefault();
    if (passwordForm.newPassword !== passwordForm.confirmNewPassword) {
      toast.error('New passwords do not match.');
      return;
    }

    setPasswordSaving(true);
    try {
      await api.put('/profile/change-password', {
        currentPassword: passwordForm.currentPassword,
        newPassword: passwordForm.newPassword,
        confirmNewPassword: passwordForm.confirmNewPassword,
      });
      toast.success('Password changed successfully.');
      setPasswordForm({ currentPassword: '', newPassword: '', confirmNewPassword: '' });
      setShowPasswordSection(false);
    } catch (err) {
      toast.error(err.response?.data?.detail || 'Failed to change password.');
    } finally {
      setPasswordSaving(false);
    }
  }

  const initial = (profile?.username || user?.username || '?')[0].toUpperCase();
  const avatarSrc = isEditing ? form.profilePictureUrl : profile?.profilePictureUrl;

  return (
    <div className="profile-modal-backdrop" onClick={onClose}>
      <div className="profile-modal-card" onClick={(e) => e.stopPropagation()}>
        {/* Header */}
        <div className="profile-modal-header">
          <div className="profile-modal-title">
            <UserIcon size={18} className="profile-title-icon" />
            <span>User Profile</span>
          </div>
          <button className="profile-modal-close" onClick={onClose} title="Close">
            <X size={18} />
          </button>
        </div>

        {loading ? (
          <div className="profile-modal-loading">
            <div className="spinner" />
          </div>
        ) : (
          <div className="profile-modal-body">
            {/* Top User Card */}
            <div className="profile-hero">
              <div className="profile-avatar-wrapper">
                {avatarSrc ? (
                  <img src={avatarSrc} alt="Profile" className="profile-avatar-img" />
                ) : (
                  <div className="profile-avatar-fallback">{initial}</div>
                )}

                {isEditing && (
                  <div className="profile-avatar-actions">
                    <button
                      type="button"
                      className="profile-photo-btn"
                      onClick={() => fileInputRef.current?.click()}
                      title="Upload photo"
                    >
                      <Camera size={14} />
                    </button>
                    {avatarSrc && (
                      <button
                        type="button"
                        className="profile-photo-btn remove"
                        onClick={handleRemovePhoto}
                        title="Remove photo"
                      >
                        <Trash2 size={14} />
                      </button>
                    )}
                    <input
                      ref={fileInputRef}
                      type="file"
                      accept="image/*"
                      style={{ display: 'none' }}
                      onChange={handlePhotoUpload}
                    />
                  </div>
                )}
              </div>

              <div className="profile-hero-info">
                <h3 className="profile-name">{profile?.username || 'User'}</h3>
                <div className="profile-badge-row">
                  <span className="profile-role-badge">
                    <Shield size={12} />
                    {profile?.role || user?.role || 'Staff'}
                  </span>
                </div>
              </div>

              {!isEditing && (
                <button
                  className="profile-edit-trigger"
                  onClick={() => setIsEditing(true)}
                  title="Edit Profile"
                >
                  <Pencil size={15} />
                  <span>Edit</span>
                </button>
              )}
            </div>

            {/* View Mode */}
            {!isEditing ? (
              <div className="profile-details-grid">
                <div className="profile-detail-item">
                  <span className="profile-detail-label">
                    <Mail size={13} /> Email Address
                  </span>
                  <div className="profile-detail-value email-value">
                    <span>{profile?.email || 'N/A'}</span>
                    <span className="locked-pill" title="Email cannot be changed">
                      <Lock size={11} /> Locked
                    </span>
                  </div>
                </div>

                <div className="profile-detail-item">
                  <span className="profile-detail-label">
                    <Phone size={13} /> Phone Number
                  </span>
                  <span className="profile-detail-value">
                    {profile?.phoneNumber || 'Not provided'}
                  </span>
                </div>

                <div className="profile-detail-item full-width">
                  <span className="profile-detail-label">
                    <MapPin size={13} /> Address
                  </span>
                  <span className="profile-detail-value">
                    {profile?.address || 'Not provided'}
                  </span>
                </div>
              </div>
            ) : (
              /* Edit Mode */
              <form onSubmit={handleSaveProfile} className="profile-edit-form">
                <div className="form-group">
                  <label className="form-label" htmlFor="pUsername">Username</label>
                  <input
                    id="pUsername"
                    className="form-input"
                    type="text"
                    value={form.username}
                    onChange={(e) => setForm((prev) => ({ ...prev, username: e.target.value }))}
                    required
                  />
                </div>

                <div className="form-group">
                  <label className="form-label" htmlFor="pEmail">
                    Email Address <span className="label-note">(Cannot be changed)</span>
                  </label>
                  <div className="input-locked-wrapper">
                    <input
                      id="pEmail"
                      className="form-input locked"
                      type="email"
                      value={profile?.email || ''}
                      disabled
                    />
                    <Lock size={14} className="locked-icon" />
                  </div>
                </div>

                <div className="form-row">
                  <div className="form-group">
                    <label className="form-label" htmlFor="pPhone">Phone Number</label>
                    <input
                      id="pPhone"
                      className="form-input"
                      type="tel"
                      placeholder="+1 (555) 000-0000"
                      value={form.phoneNumber}
                      onChange={(e) => setForm((prev) => ({ ...prev, phoneNumber: e.target.value }))}
                    />
                  </div>
                  <div className="form-group">
                    <label className="form-label" htmlFor="pAddress">Address</label>
                    <input
                      id="pAddress"
                      className="form-input"
                      type="text"
                      placeholder="13th Raven Way, Arkham"
                      value={form.address}
                      onChange={(e) => setForm((prev) => ({ ...prev, address: e.target.value }))}
                    />
                  </div>
                </div>

                <div className="profile-form-footer">
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={() => {
                      setIsEditing(false);
                      setForm({
                        username: profile?.username || '',
                        phoneNumber: profile?.phoneNumber || '',
                        address: profile?.address || '',
                        profilePictureUrl: profile?.profilePictureUrl || '',
                      });
                    }}
                  >
                    Cancel
                  </button>
                  <button type="submit" className="btn btn-primary" disabled={saving}>
                    <Check size={15} />
                    {saving ? 'Saving...' : 'Save Profile'}
                  </button>
                </div>
              </form>
            )}

            {/* Password Management Section */}
            <div className="profile-password-section">
              <button
                type="button"
                className="profile-password-toggle"
                onClick={() => setShowPasswordSection((prev) => !prev)}
              >
                <div className="profile-password-title">
                  <KeyRound size={15} />
                  <span>Change Password</span>
                </div>
                <span className="profile-password-action">
                  {showPasswordSection ? 'Hide' : 'Update'}
                </span>
              </button>

              {showPasswordSection && (
                <form onSubmit={handleChangePassword} className="password-change-form">
                  <div className="form-group">
                    <label className="form-label" htmlFor="currPass">Current Password</label>
                    <input
                      id="currPass"
                      className="form-input"
                      type="password"
                      placeholder="Enter current password"
                      value={passwordForm.currentPassword}
                      onChange={(e) =>
                        setPasswordForm((prev) => ({ ...prev, currentPassword: e.target.value }))
                      }
                      required
                    />
                  </div>

                  <div className="form-row">
                    <div className="form-group">
                      <label className="form-label" htmlFor="newPass">New Password</label>
                      <input
                        id="newPass"
                        className="form-input"
                        type="password"
                        placeholder="Min. 6 characters"
                        value={passwordForm.newPassword}
                        onChange={(e) =>
                          setPasswordForm((prev) => ({ ...prev, newPassword: e.target.value }))
                        }
                        minLength={6}
                        required
                      />
                    </div>
                    <div className="form-group">
                      <label className="form-label" htmlFor="confPass">Confirm New Password</label>
                      <input
                        id="confPass"
                        className="form-input"
                        type="password"
                        placeholder="Re-enter new password"
                        value={passwordForm.confirmNewPassword}
                        onChange={(e) =>
                          setPasswordForm((prev) => ({ ...prev, confirmNewPassword: e.target.value }))
                        }
                        minLength={6}
                        required
                      />
                    </div>
                  </div>

                  <div className="password-form-footer">
                    <button
                      type="submit"
                      className="btn btn-primary btn-sm"
                      disabled={passwordSaving}
                    >
                      {passwordSaving ? 'Updating...' : 'Update Password'}
                    </button>
                  </div>
                </form>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
