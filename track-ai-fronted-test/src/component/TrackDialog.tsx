// TrackDialog חדש שתומך בריבוי מספרים לכל מכולה לפי userId
import React, { useEffect, useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  IconButton,
  Box,
  Stack,
  Collapse,
} from "@mui/material";
import PhoneInput from "react-phone-input-2";
import "react-phone-input-2/lib/material.css";
import MoreHorizIcon from "@mui/icons-material/MoreHoriz";
import AddIcon from "@mui/icons-material/Add";
import DeleteIcon from "@mui/icons-material/Delete";
import { useSmsContext, SmsDto } from "../context/SmsContext";

interface TrackDialogProps {
  open: boolean;
  onClose: () => void;
  containerId: string;
  userId: string;
  existingEntry?: SmsDto;
  refresh: () => void;
}

export default function TrackDialog({
  open,
  onClose,
  containerId,
  userId,
  existingEntry,
  refresh,
}: TrackDialogProps) {
  const { addOrUpdatePhones, removePhones } = useSmsContext();
  const [phones, setPhones] = useState<string[]>([""]);
  const [expanded, setExpanded] = useState(false);
  const [loading, setLoading] = useState(false);
  const [initialPhones, setInitialPhones] = useState<string[]>([]);
  const [isDirty, setIsDirty] = useState(false);

  useEffect(() => {
    if (existingEntry && existingEntry.userPhones.length > 0) {
      const match = existingEntry.userPhones.find((e) => e.userId === userId);
      if (match) {
        setPhones(match.phones);
        setInitialPhones(match.phones);
      }
    } else {
      setPhones([""]);
      setInitialPhones([""]);
    }
  }, [existingEntry, userId]);

  const checkIfDirty = (current: string[]) => {
    return JSON.stringify(current) !== JSON.stringify(initialPhones);
  };

  const handlePhoneChange = (index: number, value: string) => {
    const updated = [...phones];
    updated[index] = value;
    setPhones(updated);
    setIsDirty(checkIfDirty(updated));
  };

  const handleAddPhone = () => {
    const updated = [...phones, ""];
    setPhones(updated);
    setExpanded(true);
    setIsDirty(checkIfDirty(updated));
  };

  const handleRemovePhone = (index: number) => {
    const updated = phones.filter((_, i) => i !== index);
    setPhones(updated);
    setIsDirty(checkIfDirty(updated));
  };

  const handleSave = async () => {
    if (!phones.some((p) => p.trim())) return;
    setLoading(true);
    try {
      await addOrUpdatePhones(containerId, { userId, phones });
      setInitialPhones(phones);
      setIsDirty(false);
      refresh();
      onClose();
    } catch {
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async () => {
    setLoading(true);
    try {
      await removePhones(containerId, userId);
      refresh();
      onClose();
    } catch {
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
        <DialogTitle>Track Container</DialogTitle>
        <DialogContent>
          <Stack spacing={2}>
            {/* Primary phone input */}
            <Stack direction="row" spacing={1} alignItems="center">
              <Box flex={1}>
                <PhoneInput
                  country="il"
                  value={phones[0] || ""}
                  onChange={(value) => handlePhoneChange(0, value)}
                  inputStyle={{ width: "100%" }}
                  enableSearch
                  specialLabel="Primary Phone"
                />
              </Box>
              {phones.length > 1 && (
                <IconButton onClick={() => handleRemovePhone(0)}>
                  <DeleteIcon fontSize="small" />
                </IconButton>
              )}
            </Stack>

            {/* Extra phones */}
            <Collapse in={expanded}>
              <Stack spacing={2}>
                {phones.slice(1).map((p, index) => (
                  <Stack
                    key={index + 1}
                    direction="row"
                    spacing={1}
                    alignItems="center"
                  >
                    <Box flex={1}>
                      <PhoneInput
                        country="il"
                        value={p || ""}
                        onChange={(value) =>
                          handlePhoneChange(index + 1, value)
                        }
                        inputStyle={{ width: "100%" }}
                        enableSearch
                        specialLabel={`Phone ${index + 2}`}
                      />
                    </Box>
                    <IconButton onClick={() => handleRemovePhone(index + 1)}>
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </Stack>
                ))}
              </Stack>
            </Collapse>
            {/* Add button */}
            <Button
              variant="text"
              startIcon={<AddIcon />}
              onClick={handleAddPhone}
              sx={{ mt: 1 }}
            >
              Add Phone
            </Button>
            {/* Toggle expanded */}
            {phones.length > 1 && (
              <Button
                variant="text"
                startIcon={<MoreHorizIcon />}
                onClick={() => setExpanded((prev) => !prev)}
              >
                {expanded ? "Hide extra phones" : "Show more"}
              </Button>
            )}
          </Stack>
        </DialogContent>

        <DialogActions>
          <Button
            onClick={onClose}
            disabled={loading}
            // startIcon={<CloseIcon />}
          >
            Cancel
          </Button>
          <Button
            onClick={handleDelete}
            disabled={loading || phones.length === 0}
            color="error"
          >
            Delete
          </Button>
          <Button
            onClick={handleSave}
            disabled={loading || !isDirty}
            variant="contained"
          >
            Save
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
}
