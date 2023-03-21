<template>
    <div class="row">
        <div class="col">
            <colleagues-pane pane-title="Szakellátóhely munkatársai, betöltött munkakörök"
                             :colleagues="activeColleagues"
                             flavor="success"
                             pane-type="active"
                             class="mt-3"></colleagues-pane>

            <colleagues-pane pane-title="Függőben lévő jelentkezések és meghívások"
                             :colleagues="pendingColleagues"
                             flavor="info"
                             pane-type="pending"
                             class="mt-5"></colleagues-pane>

            <colleagues-pane pane-title="Törlések, visszavonások, elutasítások"
                             :colleagues="removedColleagues"
                             flavor="danger"
                             pane-type="removed"
                             class="mt-5"></colleagues-pane>

            <!-- Modal Component -->
            <b-modal ref="invitation-modal"
                     :hide-header="true"
                     :hide-footer="true"
                     size="lg"
                     :no-close-on-backdrop="true">
                <add-colleague-form :invited-colleague="invitedColleague" />
            </b-modal>
        </div>
    </div>
</template>

<script>
import creationRequestService from "../services/creation-request-service";
import { EventBus } from "../event-bus";
import addColleagueForm from "./add-colleague-form.vue";
import colleaguesPane from "./colleagues-pane.vue";
import moment from "moment";
import axios from "axios";
import utils from "../utils";

export default {
  components: {
    addColleagueForm,
    colleaguesPane
  },
  data() {
    return {
      invitedColleague: null,
      activeColleagues: [],
      pendingColleagues: [],
      removedColleagues: []
    };
  },
  computed: {
    colleagueStatusZones() {
      return this.$store.getters.colleagueStatusZones;
    },
    urls() {
      return this.$store.state.options.urls;
    }
  },
  methods: {
    initialize() {
      creationRequestService
        .getCenterProfile(this.urls.apiUrl, {
          s: "Colleagues"
        })
        .then(response => {
          let colleagues = utils.mapColleagues(
            response.data.viewModel.colleagues
          );

          this.activeColleagues = this.separateColleagues(colleagues, "active");
          this.pendingColleagues = this.separateColleagues(
            colleagues,
            "pending"
          );
          this.removedColleagues = this.separateColleagues(
            colleagues,
            "removed"
          );
        })
        .catch(e => {
          console.warn(e);
          utils.loadingFailed();
        });
    },

    separateColleagues(colleagues, zone) {
      return colleagues.filter(
        colleague =>
          this.colleagueStatusZones[zone].indexOf(
            colleague.statusHistory[0].status
          ) > -1
      );
    },

    removeActiveColleague(e) {
      confirmationModal({
        message: "Biztos, hogy törölni szeretné a személyt a listáról?",
        okClass: "btn btn-outline-secondary",
        cancelClass: "btn btn-danger",
        callback: confirmed => {
          if (confirmed) {
            this.colleagueAction(
              e,
              "activeColleagues",
              "removedColleagues",
              () => {
                EventBus.$emit("green-zone-updated");
              }
            );
          }
        }
      });
    },

    acceptApplication(e) {
      confirmationModal({
        message: "Biztos, hogy elfogadja a jelentkezést?",
        okClass: "btn btn-success",
        callback: confirmed => {
          if (confirmed) {
            this.colleagueAction(
              e,
              "pendingColleagues",
              "activeColleagues",
              () => {
                EventBus.$emit("green-zone-updated");
              }
            );
          }
        }
      });
    },

    rejectApplication(e) {
      confirmationModal({
        message: "Biztos, hogy elutasítja a személy jelentkezését?",
        okClass: "btn btn-outline-secondary",
        cancelClass: "btn btn-danger",
        callback: confirmed => {
          if (confirmed) {
            this.colleagueAction(e, "pendingColleagues", "removedColleagues");
          }
        }
      });
    },

    cancelInvitation(e) {
      confirmationModal({
        message: "Biztos, hogy visszavonja a meghívót?",
        callback: confirmed => {
          if (confirmed) {
            this.colleagueAction(e, "pendingColleagues", "removedColleagues");
          }
        }
      });
    },

    colleagueAction(e, sourceArray, destinationArray, onUpdated = null) {
      let colleague = this[sourceArray].find(x => x.id == e.id);
      if (!colleague) {
        return;
      }

      axios
        .put(this.urls.colleagueActionUrl, {
          action: e.action,
          id: e.id
        })
        .then(response => {
          colleague.statusHistory.push(utils.mapStatusItem(response.data));
          colleague.statusHistory.sort(utils.sortStatusHistory);

          this.moveColleague(colleague, sourceArray, destinationArray);
          if (onUpdated) {
            onUpdated();
          }
        })
        .catch(e => {
          utils.alertModal(e.response.data);
        });
    },

    moveColleague(colleague, sourceArray, destinationArray) {
      this[destinationArray].push(Object.assign({}, colleague));
      this[destinationArray].sort(utils.sortColleagues);

      this[sourceArray] = this[sourceArray].filter(x => x.id != colleague.id);
    },

    showInvitationForm(invitedColleague) {
      this.invitedColleague = invitedColleague;
      this.$nextTick(() => {
        EventBus.$emit("add-colleague-form-activate");
        this.$refs["invitation-modal"].show();
      });
    },

    closeInvitationForm() {
      this.$refs["invitation-modal"].hide();
    },

    resendInvitation(e) {
      let fromRemoved = e.senderPane == "removed";
      let colleague = (fromRemoved
        ? this.removedColleagues
        : this.pendingColleagues
      ).find(x => x.id == e.id);
      if (!colleague) {
        return;
      }

      this.showInvitationForm(colleague);
    },

    onColleagueAction(e) {
      switch (e.action) {
        case "RemoveActive":
          this.removeActiveColleague(e);
          break;

        case "AcceptApplication":
          this.acceptApplication(e);
          break;

        case "RejectApplication":
          this.rejectApplication(e);
          break;

        case "ResendInvitation":
          this.resendInvitation(e);
          break;

        case "CancelInvitation":
          this.cancelInvitation(e);
          break;

        case "invite":
          this.showInvitationForm(null);
          break;
      }
    },

    onColleagueInvite(e) {
      let isNew = !e.colleague.id;
      let colleague = {
        id: isNew ? null : e.colleague.id,
        email: e.colleague.email,
        occupation: +e.colleague.occupation,
        statusHistory: []
      };

      if (e.colleague.isMember) {
        colleague.memberRightId = e.colleague.memberRightId;
      } else {
        colleague.prefix = e.colleague.prefix;
        colleague.lastName = e.colleague.lastName;
        colleague.firstName = e.colleague.firstName;
        colleague.fullName = e.colleague.fullName;
      }

      axios
        .post(this.urls.colleagueInviteUrl, colleague)
        .then(reponse => {
          let sourceIsPending = null;
          if (isNew) {
            sourceIsPending = true;
            this.pendingColleagues.push(utils.mapColleague(reponse.data));
          } else {
            let existingColleague = this.pendingColleagues.find(
              x => x.id == colleague.id
            );

            if (existingColleague) {
              sourceIsPending = true;
            } else {
              // if invitation was resended the colleague is maybe in the removed area.
              existingColleague = this.removedColleagues.find(
                x => x.id == colleague.id
              );

              if (existingColleague) {
                sourceIsPending = false;
              }
            }

            if (existingColleague) {
              Object.assign(
                existingColleague,
                utils.mapColleague(reponse.data)
              );
            }

            if (sourceIsPending === false) {
              this.moveColleague(
                existingColleague,
                "removedColleagues",
                "pendingColleagues"
              );
            }
          }

          this.pendingColleagues.sort(utils.sortColleagues);

          this.closeInvitationForm();
        })
        .catch(e => {
          EventBus.$emit("add-colleague-error", e.response.data);
        });
    },

    onViewColleagueProfile(e) {
      utils.viewColleagueProfile(this.urls.viewColleagueUrl, e.colleague);
    },

    onSave() {
      EventBus.$emit("saved");
    }
  },
  mounted() {
    this.initialize();

    EventBus.$on("save", this.onSave);
    EventBus.$on("colleague-panel-action", this.onColleagueAction);
    EventBus.$on("add-colleague-form-closing", this.closeInvitationForm);
    EventBus.$on("add-colleague-form-invite", this.onColleagueInvite);
    EventBus.$on("view-colleague-profile", this.onViewColleagueProfile);
  },
  beforeDestroy() {
    EventBus.$off("save", this.onSave);
    EventBus.$off("colleague-panel-action", this.onColleagueAction);
    EventBus.$off("add-colleague-form-closing", this.closeInvitationForm);
    EventBus.$off("add-colleague-form-invite", this.onColleagueInvite);
    EventBus.$off("view-colleague-profile", this.onViewColleagueProfile);
  }
};
</script>
